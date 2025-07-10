using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public class ESDocSADHLineExportExtraBox31AndBox44 : ESDocSADHLineExport
	{
		public static ESDocSADHLineExportExtraBox31AndBox44 New(ESCusEntryLine entryLine, BusinessObjectFactory factory, ZString box31Text, ZString box44Text)
			=> entryLine == null ? null : new ESDocSADHLineExportExtraBox31AndBox44(entryLine, factory, box31Text, box44Text);

		protected ESDocSADHLineExportExtraBox31AndBox44(ESCusEntryLine entryLine, BusinessObjectFactory factory, ZString box31Text, ZString box44Text)
			: base(entryLine, factory)
		{
			this.box31Text = box31Text;
			this.box44Text = box44Text;
		}

		readonly ZString box31Text;

		readonly ZString box44Text;

		public override ZString Box31PackagesAndDescriptionOfGoods => box31Text;

		protected override ZString Box33CommodityCodeCore => ZString.Empty;

		protected override ZString Box33ECSupplementCore => ZString.Empty;

		protected override ZString Box33ECSupplement2Core => ZString.Empty;

		protected override ZString Box34CountryOfOriginCore => ZString.Empty;

		protected override ZString Box34StateOfOriginCore => ZString.Empty;

		protected override ZString Box35GrossWeightInKGCore => ZString.Empty;

		public override ZString Box37Procedure => ZString.Empty;

		protected override ZString Box37_2ProcedureCore => ZString.Empty;

		protected override ZString Box38NetWeightInKGCore => ZString.Empty;

		protected override ZString Box39QuotaCore => ZString.Empty;

		protected override ZString GetBox40PreviousDocumentsCore(IEnumerable<PreviousDocument> previousDocuments) => ZString.Empty;

		protected override ZString Box41SupplementaryUnitsCore => ZString.Empty;

		public new ZString Box42ItemPrice => ZString.Empty;

		public override ZString Box44AddInfoAndDocuments => box44Text;

		protected override ZBool ShowBox46StatisticalValueCore => false;
	}
}
