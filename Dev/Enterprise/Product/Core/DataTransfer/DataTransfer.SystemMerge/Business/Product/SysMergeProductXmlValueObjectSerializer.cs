using System;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.DataTransfer.SystemMerge.Business.Res;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.Xml
{
	public class SysMergeProductXmlValueObjectSerializer : XmlValueObjectSerializer
	{
		public SysMergeProductXmlValueObjectSerializer(Type valueObjectType)
			: base(valueObjectType)
		{
		}

		protected override BusinessObject CreateOrUpdateFromValueObject(IValueObjectDataAdapter dataAdapter, IBusinessObjectCollection collection, IValueObject valueObject, IValueObjectImportContext context)
		{
			BusinessObject topLevelBizObj = null;

			try
			{
				context.FactoryProvider.CreateNewWithoutSave();
			}
			catch (TargetInvocationException ex)
			{
				ErrorReporter.ReportOnce($"Attempting to create a new factory from " +
					$"FactoryProvider failed. The log of transaction count increments/decrements is as follows:" +
					$"{((IDbConnected)context.FactoryProvider.Current).Connection.GetAppTransactionCountChangedLog()}", ex);
				context.Notify(new ErrorNotification(ErrorType.Error, ex.InnerException?.Message ?? ex.Message));
				return topLevelBizObj;
			}

			try
			{
				Xsd.OrgSupplierPart xsdProduct = (Xsd.OrgSupplierPart)valueObject;
				if (xsdProduct.OrgPartRelations.Cast<Xsd.OrgPartRelation>().Any(xsdRelation => context.Factory.Load<OrgHeader>(new ZGuid(xsdRelation.OrgHeaderPK)) == null))
				{
					string message = Res.GetString("fc4ce354-f738-45e9-8c59-e373d1e179d2", "Import of product [({0}) - {1} - {2}] skipped. Reason: Product references missing Organizations.",
						xsdProduct.PK.ToString(), xsdProduct.PartNum, xsdProduct.Desc) + "\r\n";
					context.Notify(new InfoNotification(message));
				}
				else if (xsdProduct.PartNum.Length > OrgSupplierPartSchema.OP_PartNum.MaxLength)
				{
					string message = Res.GetString("8D4F56BE-78D8-410E-8B53-214952A95655", "Import of product [({0}) - {1} - {2}] skipped. Reason: Product Number cannot not be longer than {3} characters.",
						xsdProduct.PK.ToString(), xsdProduct.PartNum, xsdProduct.Desc, OrgSupplierPartSchema.OP_PartNum.MaxLength);
					context.Notify(new ErrorNotification(ErrorType.Error, message));
				}
				else
				{
					topLevelBizObj = dataAdapter.CreateOrUpdateFromValueObject(valueObject, context);
					SaveIfNotInTheDatabaseAndNotify(topLevelBizObj, context);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				topLevelBizObj = null;
				context.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			}

			return topLevelBizObj;
		}

		/// <summary>
		/// Only Save Factory and Notify if the Top Level Business Object is a new imported one
		/// (as opposed to an existing one, which has been loaded and skipped from import)
		/// </summary>
		void SaveIfNotInTheDatabaseAndNotify(BusinessObject topLevelBizObj, IValueObjectImportContext context)
		{
			if (!topLevelBizObj.IsInDatabase)
			{
				try
				{
					SaveAndNotify(topLevelBizObj, context);
					context.Notify(SysMergeValueObjectHelper.GetImportedSuccessfullyNotification(GetProductDisplayInfo(topLevelBizObj)));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					string message = String.Format("{0}\r\n\r\n{1}", GetProductDisplayInfo(topLevelBizObj), ex.Message);
					throw new Exception(message, ex);
				}
			}
		}

		/// <summary>
		/// Saves and add a create business object notification in order to increment the count of imported top level objects.
		/// </summary>
		void SaveAndNotify(BusinessObject topLevelBizObj, IValueObjectImportContext context)
		{
			BusinessObjectCreatedOrUpdatedNotification createdBizObjNotification = new BusinessObjectCreatedOrUpdatedNotification(topLevelBizObj);
			context.FactoryProvider.SaveCurrentAndUpdateRecordCounts();
			context.Notify(createdBizObjNotification);
		}

		string GetProductDisplayInfo(BusinessObject topLevelBizObj)
		{
			OrgSupplierPart topLevelProduct = (OrgSupplierPart)topLevelBizObj;
			string result = Res.GetString("dc872875-92f6-4914-a613-0901802aa57a", "Product: [({0}) - {1} - {2}]",
				topLevelProduct.PK.ToString(), topLevelProduct.OP_PartNum, topLevelProduct.OP_Desc);
			return result;
		}
	}
}
