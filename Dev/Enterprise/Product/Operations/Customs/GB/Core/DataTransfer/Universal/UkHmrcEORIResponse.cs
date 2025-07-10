using System;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	class UkHmrcEORIResponse
	{
		public string Eori { get; set; }
		public bool Valid { get; set; }
		public DateTime ProcessingDate { get; set; }
	}
}
