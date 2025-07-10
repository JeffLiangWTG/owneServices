using System;

namespace Enterprise.Client.EDI.AmnestyReporter.Business
{
	class DatAmnestyFailure
	{
		public Guid AF_PK { get; set; }
		public DateTime AF_StartDate { get; set; }
		public Guid? AF_IM { get; set; }
		public Guid? EDI_IM { get; set; }
		public string Grouping { get; set; }
		public Guid E6_PK { get; set; }
		public string E6_MethodName { get; set; }
		public string E2_TestClass { get; set; }
		public string E8_AssemblyName { get; set; }
		public string ST_ResponsibleUser { get; set; }
		public string ST_Product { get; set; }
		public string ST_ProductArea { get; set; }
		public string ST_Module { get; set; }

		public DatAmnestyFailure Clone()
		{
			return (DatAmnestyFailure)MemberwiseClone();
		}
	}
}