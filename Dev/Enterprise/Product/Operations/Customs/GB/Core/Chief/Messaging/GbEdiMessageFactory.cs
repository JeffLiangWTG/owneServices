using Enterprise.Edifact;

namespace Enterprise.Customs.GB.Chief
{
	public static class GbEdiMessageFactory
	{
		static MessageFactory fGbEdifactMessageFactory;

		public static MessageFactory Factory
		{
			get
			{
				if (fGbEdifactMessageFactory == null)
				{
					var contrl_d_04a = new Edifact.D04A.D04AMessageFactory();
					contrl_d_04a.AddRegisteredMessage(
						   new MessageFactory.MessageRegistration(
							   typeof(Edifact.D04A.Messages.CONTRL.CONTRLMessage), "UN", "4", "1", "CONTRL"));

					var contrl_1_912 = new Edifact.D04A.D04AMessageFactory();  // NB CONTRL:1:912:UN is not, of course, CONTRL:D:04A:UN, but for GB their structure is the same. 
					contrl_1_912.AddRegisteredMessage(
						   new MessageFactory.MessageRegistration(
							   typeof(Edifact.D04A.Messages.CONTRL.CONTRLMessage), "UN", "1", "912", "CONTRL"));

					var contrl_4_1 = new Edifact.D04A.D04AMessageFactory();  // NB CONTRL:4:1:UN is not, of course, CONTRL:D:04A:UN, but for GB their structure is the same. 
					contrl_4_1.AddRegisteredMessage(
						   new MessageFactory.MessageRegistration(
							   typeof(Edifact.D04A.Messages.CONTRL.CONTRLMessage), "UN", "4", "1", "CONTRL"));

					var ukcinv_d_00a = new Edifact.D00A.EdifactD00AMessageFactory();
					ukcinv_d_00a.AddRegisteredMessage(
						   new MessageFactory.MessageRegistration(
							   typeof(EdiFact.UKCINV.UkCinvMessage), "UN", "D", "00A", "UKCINV"));

					var contrl_2_2 = new Edifact.D04A.D04AMessageFactory();  // NB CONTRL:4:1:UN is not, of course, CONTRL:D:04A:UN, but for GB their structure is the same. 
					contrl_2_2.AddRegisteredMessage(
						   new MessageFactory.MessageRegistration(
							   typeof(Edifact.D04A.Messages.CONTRL.CONTRLMessage), "UN", "2", "2", "CONTRL"));

					fGbEdifactMessageFactory = new MessageFactory(contrl_d_04a, contrl_1_912, contrl_4_1, contrl_2_2, ukcinv_d_00a);
				}

				return fGbEdifactMessageFactory;
			}
		}
	}
}
