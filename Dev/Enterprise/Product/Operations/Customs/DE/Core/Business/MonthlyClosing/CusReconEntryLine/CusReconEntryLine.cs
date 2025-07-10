using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using CusReconSnapshot = Enterprise.Customs.Business.CusReconSnapshot;

namespace Enterprise.Customs.DE.Business
{
	public class CusReconEntryLine : Customs.Business.CusReconEntryLine, Integration.Customs.DE.ICusReconEntryLine
	{
		public CusReconEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ReadOnly(true)]
		public override ZShort CRL_LineNumber
		{
			get => base.CRL_LineNumber;
			set => base.CRL_LineNumber = value;
		}

		[ReadOnly(true)]
		public override ZShort CRL_OriginalEntryLineNumber
		{
			get => base.CRL_OriginalEntryLineNumber;
			set => base.CRL_OriginalEntryLineNumber = value;
		}

		[ReadOnly(true)]
		public override ZString CRL_Description
		{
			get => base.CRL_Description;
			set => base.CRL_Description = value;
		}

		[ReadOnly(true)]
		public override ZString CRL_CustomsStatus
		{
			get => base.CRL_CustomsStatus;
			set => base.CRL_CustomsStatus = value;
		}

		[ReadOnly(true)]
		[ResourceStringData("43F802B7-129F-454B-A016-C9255170E115", Caption = "Status Description")]
		public ZString CustomsStatusDescription => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, CRL_CustomsStatus, Core.Constants.CountryCodes.Germany,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Now)?.ZZD_Description ?? ZString.Empty;

		[ResourceStringData("837483E1-9B33-42C6-ACF4-8EF161259969", Caption = "Has Changes?")]
		public ZString EntryLineHasChanges => CurrentSnapshot != null ? YesNoList.Descriptions.Yes : string.Empty;

		public CusReconEntry ReconEntry => Factory.GetValue(ref reconEntry, () => Factory.Load<CusReconEntry>(CRL_CRE));
		CachedProperty<CusReconEntry> reconEntry;

		public CusEntryLine EntryLine => Factory.GetValue(ref entryLine, () =>
		{
			var cusEntryHeader = Factory.Load<CusEntryHeader>(ReconEntry.CRE_CH_OriginalEntry);
			return cusEntryHeader?.MergedLines?.SingleOrDefault(x => x.CL_LineNumber == CRL_OriginalEntryLineNumber);
		});
		CachedProperty<CusEntryLine> entryLine;

		internal CusReconSnapshot CurrentSnapshot => CusReconSnapshots.FirstOrDefault(x => x.CRS_Type == CusReconConstants.Current);

		internal CusReconSnapshot LodgedSnapshot => CusReconSnapshots.FirstOrDefault(x => x.CRS_Type == CusReconConstants.Lodged);
	}
}
