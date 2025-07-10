using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.MasterFiles.Business;
using IECusEntryHeader = Enterprise.Customs.IE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.IE.DocumentWrappers
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	sealed class IEDocEAD : DocSADH
	{
		public static IEDocEAD New(IECusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) => new IEDocEAD(entryHeader, factoryToWrap);

		IEDocEAD(IECusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) : base(entryHeader, factoryToWrap)
		{
		}

		public new IECusEntryHeader EntryHeader => (IECusEntryHeader)base.EntryHeader;

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override DocSADHLineCollection GetLinesCore() => new IEDocSADHLineCollection(EntryHeader.MergedLines, Factory);

		protected override ZString Box29ExitOfficeLabelCore => Res.GetString("B83574C7-FBF7-46F6-A1CB-CD2482E8D1DC", "Office of Exit");

		protected override ZString EadBarcodeCore => EntryHeader.MovementReferenceNumber;

		protected override ZString Box7ReferenceNumberCore
		{
			get
			{
				var sb = new ZStringBuilder();
				sb.AppendIfNotEmpty(base.Box7ReferenceNumberCore);
				sb.AppendIfNotEmpty(EntryHeader.CH_BGMReference);
				sb.AppendIfNotEmpty(Declaration.JE_HouseBill);
				return sb.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.SemiColonAndspace);
			}
		}

		public ZString Box14Declarant
		{
			get
			{
				var result = ZString.Empty;
				if (ShowDeclarantRepresentativeAddress && Declaration.Declarant is OrgAddress declarant)
				{
					result = declarant.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true) + System.Environment.NewLine + DocAddress.New(declarant, Factory);
				}
				return result.Trim();
			}
		}

		public ZString Box14Representative
		{
			get
			{
				var result = ZString.Empty;
				if (ShowDeclarantRepresentativeAddress && Declaration.Representative is OrgAddress representative)
				{
					result = representative.GetEuIdentificationNumber(Core.Constants.CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true) + System.Environment.NewLine + DocAddress.New(representative, Factory);
				}
				return result.Trim();
			}
		}

		public ZString Box29ExportOffice => Declaration.JE_CustomsOffice;
	}
}
