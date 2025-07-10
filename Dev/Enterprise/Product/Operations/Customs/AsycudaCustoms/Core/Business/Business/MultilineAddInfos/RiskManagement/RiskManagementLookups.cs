using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class RiskManagementLookups : CusSupportingInfoLookups
	{
		public RiskManagementLookups(RiskManagement parent) : base(parent)
		{
		}

		new RiskManagement Parent => (RiskManagement)base.Parent;

		public new CodeDescriptionPairList CodeList => Factory.GetCachedValue<EntryPermitTypeList>();

		public OutOfRegimeCusEntryHeaderCollection EntryHeaders
		{
			get
			{
				var result = new OutOfRegimeCusEntryHeaderCollection(Factory, Parent.Parent?.JobDeclaration?.JE_MessageType ?? ZString.Empty);

				if (Parent.HasEntryNumber)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(
						ModuleEntryHeaderCollection.FilterConstants.EntryNumber,
						"Property",
						Parent.CSI_ReferenceNumber));
				}

				return result;
			}
		}
	}
}
