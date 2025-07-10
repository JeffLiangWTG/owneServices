using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCusCode : DocumentWrapper
	{
		DocCusCode(OrgCusCode orgCusCode, BusinessObjectFactory factoryToWrap)
			: base(orgCusCode, factoryToWrap)
		{
		}
		public static DocCusCode New(OrgCusCode orgCusCode, BusinessObjectFactory factoryToWrap)
		{
			if (orgCusCode == null)
			{
				return null;
			}
			else
			{
				return new DocCusCode(orgCusCode, factoryToWrap);
			}
		}

		OrgCusCode OrgCusCode
		{
			get { return (OrgCusCode)WrappedObject; }
		}
		public ZString CodeType
		{
			get { return OrgCusCode.OK_CodeType; }
		}
		public ZString CustomsRegNo
		{
			get { return OrgCusCode.OK_CustomsRegNo; }
		}

		public ZString PremisesAddress
		{
			get { return OrgCusCode.PremisesAddress?.GetFullAddressString(); }
		}
		public DocOrganisation Organisation
		{
			get { return OrgCusCode.OK_OH.IsValid ? DocOrganisation.New(OrgCusCode.Factory, OrgCusCode.OK_OH) : null; }
		}
		public DocCountry Country
		{
			get { return DocCountry.New(OrgCusCode.CodeCountry, Factory); }
		}

		public override string ToString()
		{
			return CustomsRegNo;
		}
	}
}
