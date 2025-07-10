using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Business
{
	public partial class JobDeclaration : BaseJobDeclaration
	{
		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		protected override void DefaultJE_ApplicationCodeWhenLocalCountryCustomsInterfaceIsEmpty()
		{
			if (GetInterfaceSubmissionType().IsEmpty)
			{
				JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			}
			else
			{
				base.DefaultJE_ApplicationCodeWhenLocalCountryCustomsInterfaceIsEmpty();
			}
		}

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Mexico;

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("MX"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
		public new CusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (CusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

		[ResourceStringData("Enterprise.Customs.MX.Business.JobDeclaration|JE_GoodsOrigin", Caption = "Origin Region", FullDescription = "The Goods Origin Region.")]
		public override ZString JE_GoodsOrigin { get => base.JE_GoodsOrigin; set => base.JE_GoodsOrigin = value; }

		[ResourceStringData("Enterprise.Customs.MX.Business.JobDeclaration|JE_GoodsDestination", Caption = "Destination Region", FullDescription = "The Goods Destination Region.")]
		public override ZString JE_GoodsDestination { get => base.JE_GoodsDestination; set => base.JE_GoodsDestination = value; }

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomRegimeList))]
		[ResourceStringData("Enterprise.Customs.MX.Business.JobDeclaration|JE_CustomsProfile", Caption = "Customs Regime", FullDescription = "The Customs Regime.")]
		public override ZString JE_CustomsProfile { get => base.JE_CustomsProfile; set => base.JE_CustomsProfile = value; }

		#region JE_MessageSubType

		public override ZString JE_MessageSubType
		{
			get
			{
				return base.JE_MessageSubType;
			}
			set
			{
				var oldValue = JE_MessageSubType;
				base.JE_MessageSubType = value;
				if (!IsCopying && JE_MessageSubType != oldValue)
				{
					var defaultCustomsRegime = IsImport ? MXDeclarationTypeList.GetDefaultCustomsRegimeForImport(value) : MXDeclarationTypeList.GetDefaultCustomsRegimeForExport(value);
					if (!defaultCustomsRegime.IsNullOrEmpty())
					{
						JE_CustomsProfile = defaultCustomsRegime;
					}
				}
			}
		}

		#endregion

		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore()
		{
			return new EntryInstructionProvider(this);
		}

		protected override ExchangeRateType RateTypeCore => IsExport ? ExchangeRateType.CustomsSecondary : base.RateTypeCore;

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.MX.Business.JobDeclaration|JE_LocationOfGoodsExport", ShortCaption = "Entry Area", Caption = "Entry Area", FullDescription = "The Customs Entry Area")]
		[ResourceStringData("Enterprise.Customs.MX.Business.JobDeclaration|JE_LocationOfGoodsImport", ShortCaption = "Exit Area", Caption = "Exit Area", FullDescription = "The Customs Exit Area", MultipleKey = "EXP")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.EntryAreaList))]
		public override ZString JE_LocationOfGoods { get => base.JE_LocationOfGoods; set => base.JE_LocationOfGoods = value; }

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.MX.Business.JobDeclaration|JE_SubLocationOfGoods", Caption = "Clearance Area", FullDescription = "The Customs Clearance Area")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ClearanceList))]
		public override ZString JE_SubLocationOfGoods { get => base.JE_SubLocationOfGoods; set => base.JE_SubLocationOfGoods = value; }

		protected override IReadOnlyList<string> MultipleKeysToUseCore => new string[] { JE_MessageType };

		public override void OnSaving()
		{
			base.OnSaving();

			if (!IsImport && !JE_GoodsDestination.IsEmpty)
			{
				JE_GoodsDestination = ZString.Empty;
			}
			if (!IsExport && !JE_GoodsOrigin.IsEmpty)
			{
				JE_GoodsOrigin = ZString.Empty;
			}
		}

		#region IDocumentSupport Members
		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new JobDeclarationDocumentSupporter(this);
		}
		#endregion
	}
}
