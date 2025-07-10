using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[DescriptionProperty(CACFIAEndUseCodesSchema.Constants.FE_Desc)]
	public class CACFIAEndUseCodes : AutoCACFIAEndUseCodes
	{
		public CACFIAEndUseCodes(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("63A94B30-015E-483e-8461-0FEF77B16FF4", "End Use Code: '{0}'", FE_Code); }
		}
	}
}
