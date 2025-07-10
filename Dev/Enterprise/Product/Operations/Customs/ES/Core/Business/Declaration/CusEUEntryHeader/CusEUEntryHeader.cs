using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusEUEntryHeader : EU.Business.Declaration.CusEUEntryHeader, Integration.Customs.ES.ICusEUEntryHeader
	{
		public CusEUEntryHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusEUEntryHeaderLookups Lookups => (CusEUEntryHeaderLookups)base.Lookups;

		[List(nameof(Lookups) + "." + nameof(CusEUEntryHeaderLookups.EADPrintProcedureCodeList))]
		public override ZString EUH_EADPrintProcedure { get => base.EUH_EADPrintProcedure; set => base.EUH_EADPrintProcedure = value; }

		public ZString FormattedEADPrint
		{
			get
			{
				var code = EUH_EADPrintProcedure;
				var description = (ZString)Lookups.EADPrintProcedureCodeList.GetDescriptionFromCode(EUH_EADPrintProcedure);
				return !description.IsEmpty ? description : code;
			}
		}

		protected override EU.Business.Declaration.CusEUEntryHeaderLookups GetNewLookups() => new CusEUEntryHeaderLookups(this);
	}
}
