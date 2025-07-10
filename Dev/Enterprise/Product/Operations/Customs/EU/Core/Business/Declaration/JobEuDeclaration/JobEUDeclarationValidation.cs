//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobEUDeclarationValidation
//
//    This class should be used for overriding validation in AutoJobEUDeclarationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobEUDeclarationValidation : AutoJobEUDeclarationValidation
	{
		public JobEUDeclarationValidation(AutoJobEUDeclaration parent) : base(parent)
		{
		}

		protected override void CheckEUD_AgreedPlaceCode()
		{
			base.CheckEUD_AgreedPlaceCode();
			var parent = (JobEUDeclaration)Parent;
			var codeInfo = parent.EUD_AgreedPlaceCodeInfo;

			if (parent.Declaration is JobDeclaration declaration)
			{
				if (declaration.AgreedPlaceCodeSupportAndVisible && declaration.EUD_AgreedPlaceCodeValidationSupport)
				{
					var code = parent.EUD_AgreedPlaceCode;
					if (!code.IsEmpty)
					{
						switch (code.Length)
						{
							case 5:
								ListValidation.MessageErrorIfInvalidCode(codeInfo, ResString.GetMultilingualString("54F25DC6-7DE1-4031-AC0B-AA4D91E03A76", "{0} must be a valid UNLOCODE", codeInfo.HumanReadableName));
								break;
							case 2:
								ListValidation.MessageErrorIfInvalidCode(codeInfo, ResString.GetMultilingualString("A05EEC7A-3B35-4C54-892C-29AB86C6D0B0", "{0} must be a valid country", codeInfo.HumanReadableName));
								parent?.Declaration?.Validation.ValidateJE_ShipmentIncoTermPlace();
								break;
							default:
								codeInfo.AddMessageError(ResString.GetMultilingualString("48F82AE0-F865-4F23-B4FD-B2E62BF11E0F", "{0} must either be a valid country or a valid UNLOCODE", codeInfo.HumanReadableName));
								break;
						}
					}
					else
					{
						codeInfo.AddMessageError(ResString.GetMultilingualString("22E77972-FA4B-4CBE-B0B3-049AB0DADDEB","{0} or Country Code is required", codeInfo.HumanReadableName));
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(codeInfo);
				}
			}
		}
	}
}

