using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class FreightExtensions
	{
		public static IEnumerable<JobDeclaration> GetAUDeclarations(this CommonShipment shipment)
		{
			foreach (BaseJobDeclaration declaration in shipment.Declarations)
			{
				var company = declaration.Company;
				if (company != null && company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
				{
					var auDeclaration = declaration as JobDeclaration;
					if (auDeclaration != null)
					{
						yield return auDeclaration;
					}
				}
			}
		}

		public static ZString GetContainerNumForConsol(this PackLine packLine, CommonConsol consol)
		{
			ZString result = ZString.Empty;
			if (consol != null)
			{
				var packLineContainer = packLine.GetContainer(consol);
				if (packLineContainer != null)
				{
					result = packLineContainer.JC_ContainerNum;
				}
			}
			return result;
		}

		public static IEnumerable<IEManifestLine> GetHVLVConsignmentLines(this ForwardingShipment shipment)
		{
			var result = Enumerable.Empty<IEManifestLine>();
			if (shipment.IsHighVolumeLowValue)
			{
				result = shipment.HVLVConsignments.Where(x => x.HVC_JE_ImportDeclaration.IsEmpty && x.HVC_JE_ExportDeclaration.IsEmpty);
			}
			return result;
		}
	}
}
