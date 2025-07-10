using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public partial class CDSJobDeclarationValidation
	{
		protected override void CheckJE_RouteFRequested()
		{
			base.CheckJE_RouteFRequested();
			if (Parent.JE_RouteFRequested)
			{
				Parent.JE_RouteFRequestedInfo.AddMessageError("FEC challenges under CDS are not blocking and therefore Route F is obsolete. Untick this box.");
			}
		}

		protected override void CheckJE_SpecificCircumstanceIndicator()
		{
			if (!Parent.IsImport)
			{
				base.CheckJE_SpecificCircumstanceIndicator();
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_SpecificCircumstanceIndicatorInfo, Parent.AddInfoLookups.SpecificCircumstanceIndicatorList);
			}
		}

		protected override void CheckJE_IsTrainingDeclaration()
		{
			base.CheckJE_IsTrainingDeclaration();
			if (Parent.JE_IsTrainingDeclaration)
			{
				Parent.JE_IsTrainingDeclarationInfo.AddError("CDS does not support training entries. Untick this box.");
			}
		}

		protected override void CheckJE_Gateway()
		{
			base.CheckJE_Gateway();
			if (Parent.JE_Gateway.IsEmpty)
			{
				Parent.JE_GatewayInfo.AddMessageError(CSPZG_GatewayValidationError);
			}
		}

		protected override void CheckJE_CTStatusID()
		{
			base.CheckJE_CTStatusID();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_CTStatusIDInfo);
		}

		protected override void CheckJE_Box18TransportID()
		{
			base.CheckJE_Box18TransportID();
			if (Parent.IsImport || Parent.IsExport)
			{
				var acceptedTransportModes = new ZString[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Mail, TransportTypeList.Codes.FixedTransportInstallations };
				if (!Parent.JE_TransportMode.IsEmpty && !acceptedTransportModes.Contains(Parent.JE_TransportMode))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_Box18TransportIDInfo);
				}
			}
		}
	}
}
