using CargoWise.Types;

namespace Enterprise.Customs.GB.Chief.EdiFact.UKCINV
{
	public class EmrOrErsDeclarationReport : IRoutingProvider
	{
		public ZString DeclarationUCR { get; set; }  //DECLN-UCR
		public ZString DeclarationUcrPart { get; set; }  //DECLN-PART-NO
		public ZString ImportCustomsStatus { get; set; }  //ICS
		public ZString StyleOfEntry { get; set; }  //SOE
		public ZString RouteOfEntry { get; set; }  //ROE
		public ZInt TotalPackages { get; set; }  //TOT-PKGS
		public ZDecimal TotalNetMassKilos { get; set; }  //TOT-NET-MASS
		public ZString CommodityCode { get; set; }  //CMDTY-CODE
		public ZString SubmittingRole { get; set; }  //SUBMIT-ROLE
	}
}
