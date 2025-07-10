using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobComInvoiceLineValidation : Customs.Business.BaseJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine line)
			: base(line)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public new PartAttributeValidation PartAttributeValidation => (PartAttributeValidation)base.PartAttributeValidation;

		protected override MasterFiles.Business.PartAttributeValidation GetPartAttributeValidation()
		{
			return new PartAttributeValidation();
		}

		protected override void CheckJI_Procedure()
		{
		}

		#region Overriden Checks

		protected override void CheckJI_CC()
		{
			base.CheckJI_CC();
			var classification = Parent.Classification;
			if (!Parent.JI_CC.IsEmpty && classification != null)
			{
				if (classification.CC_ClassificationType != Parent.MessageType &&
					(Parent.Declaration == null || !((Parent.Declaration.IsImportOrDrawback && classification.CC_ClassificationType == JobMessageTypeList.Codes.Import)
																				|| (Parent.Declaration.IsExport && classification.CC_ClassificationType == JobMessageTypeList.Codes.Export))))
				{
					Parent.JI_CCInfo.AddWarning("The message type set in the declaration doesn't match this classification type");
				}
			}
		}

		protected override void CheckJI_CustomsUnitQty()
		{
			base.CheckJI_CustomsUnitQty();
			var defaultUQ = Parent.Tariff?.ZZ1_ZZ8_UQ1.ToUpper() ?? ZString.Empty;
			var enteredUQ = Parent.JI_CustomsUnitQty;
			var nrCode = AUConstants.AdditionalUQCodes.NR;
			if (Parent.UseCustomsRefData && defaultUQ == nrCode && !enteredUQ.IsEmpty && enteredUQ != nrCode)
			{
				Parent.JI_CustomsUnitQtyInfo.AddWarning("The tariff does not require a unit of quantity");
			}
		}

		protected override void CheckJI_CustomsQuantity()
		{
			if (!Parent.JI_CustomsQuantityInfo.ReadOnly)
			{
				base.CheckJI_CustomsQuantity();
			}
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			if (Parent.JI_Description.IsEmpty)
			{
				Parent.JI_DescriptionInfo.AddMessageError("Goods description is required for declaration.");
			}
		}

		protected override void CheckJI_PartNo()
		{
			base.CheckJI_PartNo();
			ValidateJI_Description();
		}

		protected override void CheckJI_InvoiceUQ()
		{
			base.CheckJI_InvoiceUQ();
			if (Parent.JI_InvoiceUQ.IsEmpty)
			{
				Parent.JI_InvoiceUQInfo.AddWarning("Unit of Quantity should be entered on each line for Landed Costing purposes and Order Reconciliations.");
			}
		}

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();
			if (Parent.JI_Weight != 0m && Parent.JI_WeightUQ.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_WeightUQInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.JI_WeightUQInfo, Parent.JI_WeightUQ_List);
		}

		protected override void CheckJI_NetWeightUQ()
		{
			base.CheckJI_NetWeightUQ();
			if (Parent.IsQuarantine && Parent.InvoiceHeader.IsNEXDOCSActive && NetWeightUQNotConvertibleForNexdocs)
			{
				Parent.JI_NetWeightUQInfo.AddWarning("The selected Unit of Measure is not valid for NEXDOC and cannot be converted, please ensure a correct NEXDOC value is sent by updating REX Packages > Net Quantity.");
			}
		}

		bool NetWeightUQNotConvertibleForNexdocs
		{
			get
			{
				return Parent.JI_NetWeightUQ == Core.Constants.Weight.Kilotonnes
					|| Parent.JI_NetWeightUQ == Core.Constants.Weight.Pounds
					|| Parent.JI_NetWeightUQ == Core.Constants.Weight.PoundsTroy
					|| Parent.JI_NetWeightUQ == Core.Constants.Weight.MetricCarat
					|| Parent.JI_NetWeightUQ == Core.Constants.Weight.OuncesTroy
					|| Parent.JI_NetWeightUQ == Core.Constants.Weight.LongTons;
			}
		}

		#endregion

		#region Validation for Calculated fields

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJI_Calc_TNI();
		}

		public void ValidateJI_Calc_TNI()
		{
			ValidateCalculatedProperty(Parent.JI_Calc_TNIInfo);
		}

		protected virtual void CheckJI_Calc_TNI()
		{
		}

		public void ValidateJI_Drawback()
		{
			ValidateCalculatedProperty(Parent.JI_DrawbackInfo);
		}

		protected virtual void CheckJI_Drawback()
		{
		}

		public void ValidateJI_Texco()
		{
			ValidateCalculatedProperty(Parent.JI_TexcoInfo);
		}

		protected virtual void CheckJI_Texco()
		{
		}

		public void ValidateJI_MotorVehiclePlan()
		{
			ValidateCalculatedProperty(Parent.JI_MotorVehiclePlanInfo);
		}

		protected virtual void CheckJI_MotorVehiclePlan()
		{
			ValidateCalculatedProperty(Parent.JI_TexcoInfo);
		}

		public void ValidateJI_LinePrefix()
		{
			ValidateCalculatedProperty(Parent.JI_LinePrefixInfo);
		}

		protected virtual void CheckJI_LinePrefix()
		{
		}

		public void ValidateJI_IsPackToBondForLine()
		{
			ValidateCalculatedProperty(Parent.JI_IsPackToBondForLineInfo);
		}

		protected virtual void CheckJI_IsPackToBondForLine()
		{
		}

		public void ValidateInstrumentType()
		{
			ValidateCalculatedProperty(Parent.InstrumentTypeInfo);
		}

		protected virtual void CheckInstrumentType()
		{
		}

		public void ValidateInstrumentCode()
		{
			ValidateCalculatedProperty(Parent.InstrumentCodeInfo);
		}

		protected virtual void CheckInstrumentCode()
		{
		}

		#endregion

		#region CodeDescriptionPairLists

		public virtual RefCountryCollection CountryOfOriginList
		{
			get { return new RefCountryCollection(Parent.Factory); }
		}

		public virtual CodeDescriptionPairList InstrumentTypeList
		{
			get { return Parent.Factory.GetCachedValue<CodeDescriptionPairList>(); }
		}

		public virtual CodeDescriptionPairList InstrumentCodeList
		{
			get { return Parent.Factory.GetCachedValue<CodeDescriptionPairList>(); }
		}

		public virtual CodeDescriptionPairList LineValuationBasisList
		{
			get { return Parent.Factory.GetCachedValue<CodeDescriptionPairList>(); }
		}

		public virtual CodeDescriptionPairList LinePrefix_List
		{
			get { return Parent.Factory.GetCachedValue<CodeDescriptionPairList>(); }
		}

		public virtual CodeDescriptionPairList CustomsUQList
		{
			get { return Parent.Factory.GetCachedValue<CodeDescriptionPairList>(); }
		}

		protected CodeDescriptionPairList CombinedUQList
		{
			get
			{
				return Parent.Factory.GetCachedValue(string.Format("AU_{0}_CombinedUQList", GetType().Name),
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddRange(CustomsUQList);
						result.AddRange(Parent.JI_UQ_List);
						return result;
					});
			}
		}

		#endregion

		#region New Properties

		public virtual bool NeedsCustomsUQ
		{
			get { return false; }
		}

		public virtual ZString CustomsUQ
		{
			get { return ""; }
		}

		public virtual ZString SecondUQ
		{
			get { return ""; }
		}

		#endregion
	}
}
