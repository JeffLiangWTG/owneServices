using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class BorderTransportValidation : CusCodeDataValidation
{
	public BorderTransportValidation(BorderTransport parent)
		: base(parent)
	{
	}

	protected override void CheckCY_Data()
	{
		base.CheckCY_Data();
		ValidateTransportID();
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateNationality();
	}

	public void ValidateNationality()
	{
		ValidateCalculatedProperty(Parent.NationalityInfo);
	}

	protected new BorderTransport Parent => (BorderTransport)base.Parent;

	protected virtual void CheckNationality()
	{
		if (!Parent.CY_Data.IsEmpty)
		{
			MandatoryValidation.CheckEntered(Parent.NationalityInfo);
		}
		ListValidation.MessageErrorIfInvalidCode(Parent.NationalityInfo);
	}

	public void ValidateTransportID()
	{
		if (!Parent.CY_Data.IsEmpty
			&& Parent.Parent is JobDeclaration declaration
			&& declaration.FirstFilteredInvoiceLine is JobComInvoiceLine firstFilteredInvoiceLine
			&& (declaration.JE_TransportMode.Equals(TransportTypeList.Codes.Mail) ||
				declaration.JE_TransportMode.Equals(TransportTypeList.Codes.FixedTransportInstallations)))
		{
			var dataInfo = Parent.CY_DataInfo;
			switch (firstFilteredInvoiceLine.ProcedureCode.Left(2))
			{
				case NLConstants.ProcedureCodes._10:
				case NLConstants.ProcedureCodes._11:
				case NLConstants.ProcedureCodes._31:
					switch (declaration.JE_TransportModeInland)
					{
						case TransportTypeList.Codes.Air:
							dataInfo.AddMessageError(Res.GetString("737449C7-52E3-4F73-80C7-2E5AFD4D6B82", "[21] Flight details must be left empty"));
							break;
						case TransportTypeList.Codes.Sea:
							dataInfo.AddMessageError(Res.GetString("EDC4A0D0-27A2-46A2-909A-A0819E2A6AD6", "[21] Vessel details must be left empty"));
							break;
						default:
							dataInfo.AddMessageError(Res.GetString("15AEF205-6C7A-4564-B3BF-4D73F203705D", "[21] Transport ID must be left empty"));
							break;
					}
					break;
				case NLConstants.ProcedureCodes._21:
				case NLConstants.ProcedureCodes._22:
				case NLConstants.ProcedureCodes._76:
				case NLConstants.ProcedureCodes._77:
					dataInfo.AddMessageError(Res.GetString("8CFEBDE2-CA76-4101-834F-F64E4B4B9AD2", "[21] Transport type must be left empty"));
					break;
				default:
					break;
			}
		}
	}
}
