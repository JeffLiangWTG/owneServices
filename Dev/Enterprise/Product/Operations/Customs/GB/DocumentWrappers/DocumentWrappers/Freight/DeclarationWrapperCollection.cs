using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.DocumentWrappers.Freight
{
	public class DeclarationWrapperCollection : DocBaseWrapperCollection<DeclarationWrapper>
	{
		public DeclarationWrapperCollection(ForwardingConsol consol, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (ForwardingShipment shipment in consol.Shipments)
			{
				if (shipment.IsExport())
				{
					var euDeclarations = (from Customs.Business.BaseJobDeclaration d
											  in shipment.Declarations
										  where
										  d.Country.Code == Core.Constants.CountryCodes.UnitedKingdom
										  && d.IsExport
										  && d.IsAir
										  && d.ActiveEntryHeaders.Count > 0
										  && !d.DeclarationNumber.IsEmpty
										  select d);
					if (euDeclarations.Any())
					{
						foreach (Customs.Business.BaseJobDeclaration baseDeclaration in euDeclarations)
						{
							var gbDeclaration = baseDeclaration.Factory.Load<JobDeclaration>(baseDeclaration.PK);
							var declarationWrapper = new DeclarationWrapper(new AdsParticipantFromDeclarationHelper(gbDeclaration), factory);
							Add(declarationWrapper);
						}
					}
					else
					{
						var declarationWrapper = new DeclarationWrapper(new AdsParticipantFromShipmentHelper(shipment), factory);
						Add(declarationWrapper);
					}
				}
			}
		}
	}
}

