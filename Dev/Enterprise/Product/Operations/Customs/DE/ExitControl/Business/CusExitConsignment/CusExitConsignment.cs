using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	public class CusExitConsignment : EU.ExitControl.Business.CusExitConsignment
		, Integration.Customs.DEExitControl.ICusExitConsignment
	{
		public CusExitConsignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("BB1DFBAF-B81D-4FB2-B111-9FACBB70AAB0", Caption = "Registration Number (ext.)", MediumCaption = "Rego. No. (ext.)", ShortCaption = "Rego. No.")]
		public override ZString CXC_ReferenceNumber
		{
			get => base.CXC_ReferenceNumber;
			set => base.CXC_ReferenceNumber = value;
		}

		public override ZString CXC_LocalReference
		{
			get => base.CXC_LocalReference;
			set
			{
				var oldValue = CXC_LocalReference;
				base.CXC_LocalReference = value;
				if (!IsCopying && oldValue != CXC_LocalReference)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateCXC_MovementReference();
					}
				}
			}
		}

		[ChildEditable(true)]
		public ExitControlAdditionalInfoCollection AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					additionalInfos = new ExitControlAdditionalInfoCollection(this);
					additionalInfos.Load();
					RegisterEditableChildObject(additionalInfos);
				}
				return additionalInfos;
			}
		}
		ExitControlAdditionalInfoCollection additionalInfos;

		public new CusExitConsignmentValidation Validation => (CusExitConsignmentValidation)base.Validation;

		protected override ExitControlBase.Business.CusExitConsignmentValidation GetNewValidation() => new CusExitConsignmentValidation(this);

		public new ExitControlBase.Business.ICusExitConsignmentItemCollection<CusExitConsignmentItem> CusExitConsignmentItems => (ExitControlBase.Business.ICusExitConsignmentItemCollection<CusExitConsignmentItem>)base.CusExitConsignmentItems;
		protected override ExitControlBase.Business.ICusExitConsignmentItemCollection<ExitControlBase.Business.CusExitConsignmentItem> CreateNewCusExitConsignmentItemCollection() => new ExitControlBase.Business.CusExitConsignmentItemCollection<CusExitConsignmentItem>(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(ExitControlAdditionalInfo));
			return result;
		}

		public override bool ConsignmentItemsRequiredToCreateCusExitReport
		{
			get
			{
				var consignmentItemsRequired = true;
				if (CXC_Status.IsEmpty)
				{
					consignmentItemsRequired = false;
				}
				else if (int.TryParse(CXC_Status, out int cxcStatusAsInt) && cxcStatusAsInt < 310)
				{
					consignmentItemsRequired = false;
				}

				return consignmentItemsRequired;
			}
		}
	}
}
