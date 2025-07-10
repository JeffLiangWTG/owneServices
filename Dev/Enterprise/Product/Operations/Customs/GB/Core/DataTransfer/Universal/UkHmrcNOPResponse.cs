using System;
using System.Collections.Generic;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	class UkHmrcNOPResponse
	{
		public DateTime Date { get; set; }
		public List<EoriType> Eoris { get; set; }
	}

	class EoriType
	{
		public string Eori { get; set; }
		public bool Authorised { get; set; }
	}
}
