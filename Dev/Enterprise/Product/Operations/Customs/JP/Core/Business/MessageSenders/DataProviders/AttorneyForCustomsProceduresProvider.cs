using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Business;

sealed class AttorneyForCustomsProceduresProvider : IAttorneyForCustomsProcedures
{
	public AttorneyForCustomsProceduresProvider(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}
	readonly JobDeclaration declaration;

	public string Code
	{
		get
		{
			IEnumerable<ZString> GetAttorneyForCustomsProceduresCodes()
			{
				if (string.IsNullOrEmpty(ReceiptNumber))
				{
					var representative = declaration.Representative;

					if (representative != null)
					{
						yield return representative.CustomsCodes.GetCustomsRegNo(OrgCusCode.JapanCodeTypes.LPC, Core.Constants.CountryCodes.Japan);
						yield return representative.CustomsCodes.GetCustomsRegNo(OrgCusCode.JapanCodeTypes.CIE, Core.Constants.CountryCodes.Japan);
					}
				}
			}

			return GetAttorneyForCustomsProceduresCodes().FirstOrDefault(c => !c.IsEmpty);
		}
	}

	public string ReceiptNumber => declaration.JE_ACP_POA;

	public string Name => !string.IsNullOrEmpty(ReceiptNumber) ? declaration.Representative?.CompanyName : string.Empty;
}
