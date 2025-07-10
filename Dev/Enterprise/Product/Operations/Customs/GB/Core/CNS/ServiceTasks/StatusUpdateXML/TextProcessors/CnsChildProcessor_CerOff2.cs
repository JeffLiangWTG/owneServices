using CargoWise.Types;

namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	class CnsChildProcessor_CerOff2 : CnsChildProcessor
	{
		//  UNIT OFFLOAD NOTIFICATION                
		protected override ZString RegexPatternForUcn
		{
			get { return @"UCN: ([A-Za-z0-9]{4} [A-Za-z0-9]{5} ?\d{0,3} ?\d{0,2})"; }
		}

		protected override ZString ManipulateUcn(string crappyUcnIn)
		{
			return crappyUcnIn.Replace(" ", "");
		}
	}
}
