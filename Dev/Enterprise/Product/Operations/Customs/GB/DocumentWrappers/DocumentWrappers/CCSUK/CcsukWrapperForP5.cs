using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	public class CcsukWrapperForP5 : CcsukWrapper
	{
		readonly FsaResponseMessage report;
		public CcsukWrapperForP5(EDIMessage ediMessage, BusinessObjectFactory factoryToWrap)
			: base(ediMessage, ediMessage.EM_LinkedObject, factoryToWrap)
		{
			var parser = new FsaParser(ediMessage);
			report = parser.Parse();
		}

		protected override Event EventCode => NumberOfPiecesReleasedHelper.ShedEvent; // P5 only sent to shed

		public ZString TempStorageDate => string.Empty; // Not sure what field to use at the moment

		public ZString OldAWB => report.GetJobNumberIncludingShed(OldStr);

		public ZString OldAgent
		{
			get
			{
				var oldConsignment = GetConsignment(OldStr);
				return oldConsignment != null ? oldConsignment.AgentCode : (ZString)string.Empty;
			}
		}

		public ZString NewAWB => report.GetJobNumberIncludingShed(NewStr);

		public ZString NewAgent
		{
			get
			{
				var newConsignment = GetConsignment(NewStr);
				return newConsignment != null ? newConsignment.AgentCode : (ZString)string.Empty;
			}
		}

		public ZString ShedOrAirport
		{
			get
			{
				var oldAirportShed = report.GetAirportAndShed(OldStr);
				var newAirportShed = report.GetAirportAndShed(NewStr);

				if (!oldAirportShed.SubstringSafe(0, 3).Equals(newAirportShed.SubstringSafe(0, 3)))
				{
					return Airport;
				}
				else if (!oldAirportShed.SubstringSafe(3, 3).Equals(newAirportShed.SubstringSafe(3, 3)))
				{
					return Shed;
				}

				return string.Empty;
			}
		}

		FsaChildConsignment GetConsignment(ZString oldOrNewIndicator) => (from FsaChildConsignment c in report.ChildConsignments where c.OldOrNewDataIndicator == oldOrNewIndicator select c).FirstOrDefault();

		const string Airport = "Airport";
		const string Shed = "Shed";

		const string OldStr = "OLD";
		const string NewStr = "NEW";
	}
}
