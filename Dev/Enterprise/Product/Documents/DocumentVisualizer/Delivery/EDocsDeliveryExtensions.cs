using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public static class EDocsDeliveryExtensions
	{
		#region AddCopyToEDocs

		public static bool AddCopyToEDocs(this IDocument document, IEDocsDeliveryParameters parameters)
		{
			if (document == null
				|| parameters == null)
			{
				return false;
			}

			if (parameters.BusinessObject == null
				|| !(parameters.BusinessObject is IDocManagerSupport docManagerSupportable)
				|| docManagerSupportable.DocManagerInfo == null)
			{
				return false;
			}

			var factory = parameters.Factory ?? new BusinessObjectFactory();

			var preprocessedDelivery = CreateDeliveryInfoAndMethod(
				parameters.BusinessObject,
				docManagerSupportable.DocManagerInfo,
				factory);

			var deliveryInfo = preprocessedDelivery.DeliveryInfo;

			var emailFormatter = new DocumentEmailFormatter();
			var documentName = CreateDocumentName(parameters.DocumentName, parameters.DocumentTitle);

			deliveryInfo.FileFormat = parameters.FileFormat
				?? DeliveryExtensions.ExcelWorksheetFileExtension;
			deliveryInfo.Name = documentName;
			deliveryInfo.EmailSubjectLine = emailFormatter.GetEmailSubjectLine(documentName);
			deliveryInfo.EmailSignature = emailFormatter.GetEmailSignature(documentName);
			deliveryInfo.DocumentType = parameters.DocumentType;
			deliveryInfo.AttachedFilename = parameters.AttachedFileName;
			deliveryInfo.ShowDraftWatermark = parameters.ShowDraftWatermark;

			var xls = document.ToXlsFile();
			xls.Save(deliveryInfo.FileContents);

			var method = preprocessedDelivery.Method;

			method.AddFile(deliveryInfo);
			method.Deliver();

			var deliveryGroup = factory.Load<StmDeliveryGroup>(deliveryInfo.DeliveryGroupID);
			if (deliveryGroup != null)
			{
				deliveryGroup.SB_IsProcessed = true;
			}

			if (parameters.Factory == null)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
			}

			return true;
		}

		#endregion

		#region CreateDeliveryInfoAndMethod

		static PreprocessedDelivery CreateDeliveryInfoAndMethod(IBusiness biz, DocManagerInfo docManagerInfo, BusinessObjectFactory factory)
		{
			var instructions = new DeliveryInstructions(new FactoryStrategy.PopulateButDoNotSave(factory))
			{
				Destination = DeliveryInstructionDestination.DocManager
			};

			var method = DeliveryMethod.FromContact(null, instructions);

			var deliveryGroup = instructions.DeliveryGroups.FirstOrDefault();

			if (deliveryGroup == null)
			{
				deliveryGroup = instructions.Factory.New<StmDeliveryGroup>();
				instructions.DeliveryGroups.Add(deliveryGroup);
			}

			var deliveryInfo = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document)
			{
				DeliveryGroupID = deliveryGroup.PK,
				Instructions = instructions,
				Copies = 1,
				ParentGuid = biz.Identifier,
				ParentTableName = biz.TableName,
				RelatedBusinessContext = docManagerInfo.DocManagerCode
			};

			return new PreprocessedDelivery(deliveryInfo, method);
		}

		static string CreateDocumentName(string name, string title)
		{
			return string.Compare(name, title, StringComparison.OrdinalIgnoreCase) != 0
				? string.Format(CultureInfo.InvariantCulture, "{0} ({1})", name, title)
				: name;
		}

		#endregion

		#region Nested Types

		sealed class PreprocessedDelivery
		{
			public PreprocessedDelivery(DeliveryInfo info, DeliveryMethod method)
			{
				DeliveryInfo = info;
				Method = method;
			}

			public DeliveryInfo DeliveryInfo { get; }
			public DeliveryMethod Method { get; }
		}

		#endregion
	}
}
