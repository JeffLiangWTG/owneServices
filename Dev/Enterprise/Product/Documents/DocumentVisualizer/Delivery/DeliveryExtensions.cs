using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Delivery
{
	static class DeliveryExtensions
	{
		public const string ExcelWorksheetFileExtension = "XLS";

		public static class DeliveryGroupMatchStrategies
		{
			public static Func<StmDeliveryGroup, bool> MatchAny => _ => true;
			public static Func<StmDeliveryGroup, bool> MatchByEmailSubjectLine(string emailSubject) => deliveryGroup =>
					string.IsNullOrWhiteSpace(deliveryGroup.SB_EmailSubjectLine)
					|| deliveryGroup.SB_EmailSubjectLine == emailSubject;
		}

		public static StmDeliveryGroup GetOrCreateDeliveryGroup(this DeliveryInstructions instructions, DocDeliveryContact contact, Func<StmDeliveryGroup, bool> deliveryGroupMatchStrategy = null)
		{
			if (instructions == null
				|| !instructions.UsesDeliveryGroup
				|| contact == null)
			{
				return null;
			}

			var bizObj = instructions.DocPack?.DocumentSupporter?.BusinessObject;
			var emailSubjectTemplate = instructions.DocPack?.StmMenuCommand?.SU_EmailSubjectLine;

			var emailSubject = GetDeliveryGroupEmailSubject(contact, instructions, bizObj, emailSubjectTemplate);

			if (deliveryGroupMatchStrategy == null)
			{
				deliveryGroupMatchStrategy = DeliveryGroupMatchStrategies.MatchByEmailSubjectLine(emailSubject);
			}

			var res = instructions
				.DeliveryGroups
				.FirstOrDefault(deliveryGroupMatchStrategy);

			void PopulateEmailSubjectLine(StmDeliveryGroup deliveryGroup)
			{
				if (!string.IsNullOrEmpty(emailSubject))
				{
					deliveryGroup.SB_EmailSubjectLine = emailSubject;
				}
			}

			if (res == null)
			{
				var factory = new BusinessObjectFactory();
				factory.RefreshEnabled = false;

				var deliveryGroup = factory.New<StmDeliveryGroup>();
				PopulateEmailSubjectLine(deliveryGroup);

				factory.Save();

				res = instructions.Factory.ImportFromAnotherFactorySafe(deliveryGroup);
				instructions.DeliveryGroups.Add(res);
			}
			else if (string.IsNullOrWhiteSpace(res.SB_EmailSubjectLine))
			{
				PopulateEmailSubjectLine(res);
			}

			return res;
		}

		static ZString GetDeliveryGroupEmailSubject(DocDeliveryContact contact, DeliveryInstructions instructions, BusinessObject bizObj, string emailSubjectLine)
		{
			var result = string.Empty;

			if (DeliveryMethodHelper.IsEmail(contact.DeliveryMethod) && contact != null && !contact.EmailSubjectMacro.IsEmpty)
			{
				result = ObjectFactory.Get<ITextMacroProcessor>().Replace(contact.EmailSubjectMacro, instructions.GetRelatedBusinessObjectsForEmailSubject(contact));
			}

			if (string.IsNullOrWhiteSpace(result) && bizObj != null && !string.IsNullOrEmpty(emailSubjectLine))
			{
				var formEmailSubjectEvaluator = new EmailSubjectEvaluator();

				if (formEmailSubjectEvaluator.TryCreateEmailSubject(bizObj, emailSubjectLine, out var emailSubject))
				{
					result = emailSubject;
				}
			}

			return result.Length > StmDeliveryGroupSchema.SB_EmailSubjectLine.MaxLength
				? result.Substring(0, StmDeliveryGroupSchema.SB_EmailSubjectLine.MaxLength)
				: result;
		}

		public static string GetEmailSubjectLine(this IDocumentDeliverable deliverable)
		{
			return new DocumentEmailFormatter().GetEmailSubjectLine(deliverable?.DocumentName);
		}

		public static string GetEmailSignature(this IDocumentDeliverable deliverable)
		{
			return new DocumentEmailFormatter().GetEmailSignature(deliverable?.DocumentName);
		}
	}
}
