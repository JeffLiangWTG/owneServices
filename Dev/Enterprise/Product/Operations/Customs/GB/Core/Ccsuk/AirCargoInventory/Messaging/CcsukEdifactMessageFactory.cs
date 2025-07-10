using System;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.CUSRES_2_912;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.FSA;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.Edifact;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public static class CcsukEdifactMessageFactory
	{
		static MessageFactory factory;

		public static MessageFactory Factory
		{
			get
			{
				if (factory == null)
				{
					// Note, we use D04A for 1:912, because this directory just doesn't exist
					var contrl_1_912 = new Edifact.D04A.D04AMessageFactory();
					contrl_1_912.AddRegisteredMessage(
						   new MessageFactory.MessageRegistration(
							   typeof(Edifact.D04A.Messages.CONTRL.CONTRLMessage), "UN", "1", "912", "CONTRL"));

					var genral = new Edifact.D00A.EdifactD00AMessageFactory();
					genral.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(GenralMessage), "UN", "0", "912", GenralMessageGenerator.GenralMessageCode));
					genral.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(GenralMessage), "BT", "0", "912", GenralMessageGenerator.GenralMessageCode));

					var cukfsaBt = new Edifact.D00A.EdifactD00AMessageFactory();
					cukfsaBt.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(CUKFSAMessage), "BT", "1", "912", "CUKFSA"));
					cukfsaBt.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(CUKFSAMessage), "BT", "2", "912", "CUKFSA"));
					cukfsaBt.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(CUKFSAMessage), "BT", "3", "912", "CUKFSA"));
					cukfsaBt.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(CUKFSAMessage), "BT", "4", "912", "CUKFSA"));

					var cukfsaUn = new Edifact.D00A.EdifactD00AMessageFactory();
					cukfsaUn.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(CUKFSAMessage), "UN", "1", "912", "CUKFSA"));

					var cuscar = new Edifact.D00A.EdifactD00AMessageFactory();
					cuscar.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(CUSCARMessage), "UN", "3", "912", "CUSCAR"));
					cuscar.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(CUSCARMessage), "UN", "2", "912", "CUSCAR"));
					cuscar.AddRegisteredMessage(new MessageFactory.MessageRegistration(typeof(CUSCARMessage), "UN", "1", "912", "CUSCAR"));

					var cargoFactExtractor = new Edifact.D00A.EdifactD00AMessageFactory();
					cargoFactExtractor.AddRegisteredMessage(new CargoFactMessageRegistration(typeof(CargoFactMessage), "IA", "0", "0", "CIM"));
					cargoFactExtractor.AddRegisteredMessage(new CargoFactMessageRegistration(typeof(CargoFactMessage), "Z1", "0", "0", "CIM"));
					cargoFactExtractor.AddRegisteredMessage(new CargoFactMessageRegistration(typeof(CargoFactMessage), "IA", "2", "0", "CIM"));
					cargoFactExtractor.AddRegisteredMessage(new CargoFactMessageRegistration(typeof(CargoFactMessage), "Z1", "2", "0", "CIM"));
					cargoFactExtractor.AddRegisteredMessage(new CargoFactMessageRegistration(typeof(CargoFactMessage), "Z1", "6", "0", "CIM"));
					cargoFactExtractor.AddRegisteredMessage(new CargoFactMessageRegistration(typeof(CargoFactMessage), "IA", "6", "0", "CIM"));

					var cusres = new Edifact.D00A.EdifactD00AMessageFactory();
					cusres.AddRegisteredMessage(new MessageFactory.MessageRegistration(
							   typeof(CUSRESMessage), "UN", "2", "912", "CUSRES"));

					factory = new MessageFactory(contrl_1_912, genral, cukfsaBt, cukfsaUn, cuscar, cargoFactExtractor, cusres);
				}

				return factory;
			}
		}

		class CargoFactMessageRegistration : MessageFactory.MessageRegistration
		{
			public CargoFactMessageRegistration(Type messageType, string controllingAgency, string messageTypeVersionNumber, string messageTypeReleaseNumber, string messageCode)
				: base(messageType, controllingAgency, messageTypeVersionNumber, messageTypeReleaseNumber, messageCode)
			{ }

			public override bool IsMatchForMessageCode(string currentMessageCode, string registeredMessageCode)
			{
				return currentMessageCode.StartsWith(registeredMessageCode); // allows registration CIM to load CIMFSN, CIMXXX, CIMYYY, etc
			}
		}
	}
}
