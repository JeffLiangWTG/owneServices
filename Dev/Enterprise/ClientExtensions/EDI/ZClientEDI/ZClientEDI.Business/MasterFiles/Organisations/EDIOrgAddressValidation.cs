using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgAddressValidation : OrgAddressValidation
	{
		public EDIOrgAddressValidation(EDIOrgAddress address)
			: base(address)
		{
		}

		public new EDIOrgAddress Parent
		{
			get { return (EDIOrgAddress)base.Parent; }
		}

		protected override void CheckOA_RL_NKRelatedPortCode()
		{
			base.CheckOA_RL_NKRelatedPortCode();
			if (Parent.AddressCapability.GetCapabilityEnabled(OrgAddressType.Office.Code))
			{
				MandatoryValidation.CheckEntered(Parent.OA_RL_NKRelatedPortCodeInfo);
			}
		}

		public void ValidateMainAddress()
		{
			var message = "This address is imported from client system and it shouldn't be marked as main address.";
			Parent.RemoveRowError(message);
			if (Parent.IsLinkedToClientBranch && Parent.CapabilitiesCollection.Cast<OrgAddressCapability>().Any(x => x.PZ_IsMainAddress))
			{
				Parent.AddRowError(message);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateMainAddress();
		}
	}
}


