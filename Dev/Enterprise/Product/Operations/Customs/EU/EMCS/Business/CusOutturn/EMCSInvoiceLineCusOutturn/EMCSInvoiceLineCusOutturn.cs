using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSInvoiceLineCusOutturn : CusOutturn
		, EUEMCS.IInvoiceLineCusOutturn
		, ICusCodeDataTypeSupporter
	{
		public EMCSInvoiceLineCusOutturn(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static EMCSInvoiceLineCusOutturn New(BusinessObjectFactory factory, BusinessObject parent) => New<EMCSInvoiceLineCusOutturn>(factory, parent);

		public new EMCSJobComInvoiceLine Parent => (EMCSJobComInvoiceLine)base.Parent;

		#region Properties

		[ResourceStringData("E3FC1432-0F18-4D39-ABE5-FD4BB763298E", Caption = "Refused Quantity")]
		public override ZDecimal C5_RejectedQuantity
		{
			get => base.C5_RejectedQuantity;
			set => base.C5_RejectedQuantity = value;
		}

		[ResourceStringData("6C416951-754A-4A8E-A4F5-541E13F1D616", Caption = "Quantity")]
		[DecimalPlaces(3)]
		public ZDecimal ActualQuantity => ((ZDecimal)(Parent.JI_CustomsQuantity - C5_RejectedQuantity)).Round(3);

		public ZPropertyInfo ActualQuantityInfo => GetZPropertyInfo(nameof(ActualQuantity));

		public ZString UnitQuantity => Parent.JI_CustomsUnitQty;

		public ZPropertyInfo UnitQuantityInfo => GetZPropertyInfo(nameof(UnitQuantity));

		[ResourceStringData("091C8D04-5B91-49FA-BBE4-D9297994D9AC", Caption = "Observed Difference")]
		[DecimalPlaces(3)]
		public ZDecimal ObservedDifference => ((ZDecimal)(Parent.JI_CustomsQuantity - Parent.ZG_DeclaredValue)).Round(3);

		public ZPropertyInfo ObservedDifferenceInfo => GetZPropertyInfo(nameof(ObservedDifference));

		[ResourceStringData("3C8357FD-DB2F-4ACD-A790-DE9A4266F84D", Caption = "Explanation")]
		public override ZString C5_OutturnResultReason
		{
			get => base.C5_OutturnResultReason;
			set => base.C5_OutturnResultReason = value;
		}

		#endregion

		#region Overrides

		protected override TypeLoaderCollection GetParentLoaders()
		{
			var result = base.GetParentLoaders();
			result.Add(new TypeLoader(typeof(EMCSJobComInvoiceLine)));
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			C5_ApplicationCode = CusOutturnApplicationCodeList.Codes.EMC;
		}

		public new EMCSInvoiceLineCusOutturnValidation Validation => (EMCSInvoiceLineCusOutturnValidation)base.Validation;

		protected override CusOutturnValidation GetNewValidation() => new EMCSInvoiceLineCusOutturnValidation(this);

		#endregion

		#region ReportOfReceiptReasons

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public ReportOfReceiptReasonCollection ReportOfReceiptReasons
		{
			get
			{
				if (reportOfReceiptReasons == null)
				{
					reportOfReceiptReasons = new ReportOfReceiptReasonCollection(this);
					RegisterEditableChildObject(reportOfReceiptReasons);
					reportOfReceiptReasons.Load();
				}

				return reportOfReceiptReasons;
			}
		}

		ReportOfReceiptReasonCollection reportOfReceiptReasons;

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		#region CusCodeDataTypeSupporter
		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>() { };
			result.Add(CusCodeDataTypeList.Codes.ReportOfReceiptReason, typeof(ReportOfReceiptReason));
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}
		#endregion
	}
}
