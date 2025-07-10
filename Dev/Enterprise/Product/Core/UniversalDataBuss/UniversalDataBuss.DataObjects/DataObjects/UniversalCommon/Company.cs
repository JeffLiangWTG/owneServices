using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class Company : ICodeNameDataObject
	{
		public static Company New(IGlbCompany companyBO)
		{
			return companyBO == null ? null : new Company()
			{
				Code = companyBO.GC_Code,
				Country = Country.New(companyBO.Country),
				Name = companyBO.GC_Name,
			};
		}

		[MaxLength(3), Mandatory]
		public ZString? Code { get; set; }
		public Country Country { get; set; }
		[MaxLength(100)]
		public ZString? Name { get; set; }
	}
}
