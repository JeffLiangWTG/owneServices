using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class JobDeclarationValidation : BaseJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration
		{
			get { return Parent; }
		}

		protected new JobDeclaration Parent
		{
			get { return (JobDeclaration)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCalculatedFreightAmount();
			ValidateEffectiveCCN();
		}

		public void ValidateCalculatedFreightAmount()
		{
			ValidateCalculatedProperty(Parent.CalculatedFreightAmountInfo);
		}

		public const decimal MaxTotalFreightAmount = 100000m;

		protected virtual void CheckCalculatedFreightAmount()
		{
			var b3EntryHeader = Parent.B3EntryHeader;
			if (b3EntryHeader != null)
			{
				foreach (var b3SubHeader in new B3ImportMessageWrapper(b3EntryHeader).PositiveB3SubHeaders)
				{
					if (b3SubHeader.FreightCharges >= MaxTotalFreightAmount)
					{
						Parent.CalculatedFreightAmountInfo.AddMessageError(TotalFreightIsOverAmount);
						break;
					}
				}
			}
		}

		internal static string TotalFreightIsOverAmount
		{
			get { return Res.GetString("1F4AA059-A308-442C-B000-A7524C670917", "Either the total freight entered on invoice headers or this calculated freight must be < $100,000"); }
		}

		protected override Customs.Business.ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}

		protected override void CheckJE_MessageType()
		{
			if ((ZString)Parent.JE_MessageTypeInfo.OriginalValue != Parent.JE_MessageType && Parent.DeclarationMessagesHaveBeenSent(reloadMessages: true))
			{
				Parent.JE_MessageTypeInfo.AddError(Res.GetString("644F5DB7-A2B2-4EE7-84B1-31B1D1EA5551", "You may not change the shipment type because messages have been sent. If you do need to change then cancel/withdraw any lodged declaration and reset this declaration from the Brokerage menu."));
			}
			else
			{
				base.CheckJE_MessageType();
				if (Parent.NeedValidateMessageType)
				{
					ListValidation.ErrorIfInvalidCode(Parent.JE_MessageTypeInfo, Parent.Lookups.EditableMessageTypeList);
				}
			}
		}

		protected override void CheckJE_GB()
		{
			base.CheckJE_GB();
			var branch = Parent.Branch;
			if (branch != null && !branch.GB_RL_NKHomePort.StartsWith(Core.Constants.CountryCodes.Canada, System.StringComparison.OrdinalIgnoreCase))
			{
				Parent.JE_GBInfo.AddMessageError(Res.GetString("98e01c8a-d2a5-48e4-81bc-9df48e83d2a9", "The branch location should be Canada."));
			}
		}

		protected override void CheckJE_TotalWeight()
		{
			base.CheckJE_TotalWeight();
			if (!Parent.IsLVS && !Parent.IsB3X && !Parent.IsB2Adjustments)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.JE_TotalWeightInfo, Res.GetString("d4c5d84d-22bf-4069-84a1-cc1ff695f1f4", "Total Weight"));
			}
		}

		public void ValidateEffectiveCCN()
		{
			ValidateCalculatedProperty(Parent.EffectiveCCNInfo);
		}

		protected void CheckEffectiveCCN()
		{
			var parent = Parent;
			if (parent.IsIID && parent.JE_MessageType != JobMessageTypeList.Codes.ImportCopyforB2)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.EffectiveCCNInfo);
			}

			if (!(parent.CargoControlNumbers.Count > 1) && !parent.EffectiveCCN.IsEmpty)
			{
				ReleaseStatusValidation.ValidateCCNNumber(parent.EffectiveCCNInfo, parent.EffectiveCCN, parent, false);
			}
		}

		public void ValidateEffectiveCCNPrefix()
		{
			ValidateCalculatedProperty(Parent.EffectiveCCNPrefixInfo);
		}
		protected void CheckEffectiveCCNPrefix()
		{
			var parent = Parent;
			if (!(parent.CargoControlNumbers.Count > 1))
			{
				if (parent.IsIID && parent.JE_MessageType != JobMessageTypeList.Codes.ImportCopyforB2)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.EffectiveCCNPrefixInfo);
				}

				if (!parent.EffectiveCCNPrefix.IsEmpty && parent.EffectiveCCNPrefix.Length != 4)
				{
					parent.EffectiveCCNPrefixInfo.AddMessageError(Res.GetString("4D5D4153-668D-48C9-A30E-3803528540A0", "The CCN-prefix should be composed of 4 characters"));
				}
				if (!parent.EffectiveCCN.IsEmpty)
				{
					ReleaseStatusValidation.ValidateDuplicatedCCNNumber(parent.EffectiveCCNPrefixInfo, parent.EffectiveCCN, parent, false);
				}
			}
		}

		public void ValidateEffectiveCCNSuffix()
		{
			ValidateCalculatedProperty(Parent.EffectiveCCNSuffixInfo);
		}
		protected void CheckEffectiveCCNSuffix()
		{
			var parent = Parent;
			if (!(parent.CargoControlNumbers.Count > 1))
			{
				if (parent.IsIID && parent.JE_MessageType != JobMessageTypeList.Codes.ImportCopyforB2)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.EffectiveCCNSuffixInfo);
				}

				if (!parent.EffectiveCCN.IsEmpty)
				{
					ReleaseStatusValidation.ValidateDuplicatedCCNNumber(parent.EffectiveCCNSuffixInfo, parent.EffectiveCCN, parent, false);
				}
			}
		}

		public void ValidateExamLocationDescription()
		{
			ValidateCalculatedProperty(Parent.ExamLocationDescriptionInfo);
		}

		protected void CheckExamLocationDescription()
		{
			if (!Declaration.IsLVS)
			{
				if (Declaration.IsIID && (Parent.CA_ExamLocationCode.IsEmpty && Parent.CA_ExamLocationName.IsEmpty))
				{
					Parent.ExamLocationDescriptionInfo.AddMessageError(ExamLocationMustBeEntered);
				}
			}

			if (Parent.ExamLocationDescription.HasCharactersNotSupportedByCAMessaging())
			{
				Parent.ExamLocationDescriptionInfo.AddWarning(Res.GetString("f9f5bc91-37de-4ab6-9d84-14ae1d5737b7", "The Exam Location has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs."));
			}
		}

		protected override void CheckJE_DateOfFirstArrival()
		{
			base.CheckJE_DateOfFirstArrival();

			if (Parent.IsIID)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_DateOfFirstArrivalInfo);
			}
			if (Parent.JE_DateOfFirstArrival.IsValid
				&& (Parent.JE_DateOfFirstArrival - ZDateTime.Now).TotalDays > 30
				&& Parent.HasInvoiceLinesWithGACPGA)
			{
				var message = Res.GetString("b8ea4256-965f-4089-8f97-ef940bc4dcbe", "Declarations containing goods regulated by {0} may not be accepted if provided more than 30 days in advance of arrival.", PGACodes.Descriptions.GAC);
				Parent.JE_DateOfFirstArrivalInfo.AddMessageError(message);
			}
		}

		internal static string ExamLocationMustBeEntered
		{
			get
			{
				return Res.GetString("A0B8F27F-F228-4908-A415-E4B168A1CBEE", "You must enter either a exam location code or a text description.");
			}
		}

		protected override bool ShouldValidatePackagesActualPackageCount
		{
			get
			{
				var declaration = Declaration;
				return base.ShouldValidatePackagesActualPackageCount
						&& !declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking
						&& !declaration.SupportsChcPivotBetweenInvoiceLineAndPacking;
			}
		}
	}
}
