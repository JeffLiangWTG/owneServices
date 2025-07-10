using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CIMDRP : CIMFFM
	{
		public CIMDRP(DropOffWrapper wrapper, ErrorCollector ec)
			: base(ec)
		{
			this.wrapper = wrapper;
		}

		public override ZString CargoImpCode
		{
			get { return "DRP"; }
		}

		protected override ZInt CargoImpVersionCore
		{
			get { return 8; }
		}

		protected override string[] CargoImpLinesWithoutType
		{
			get { return Regex.Split(WrapperNeat, "\r\n"); }
		}

		public override ZString MessageInterpretation
		{
			get { return WrapperNeat; }
		}

		string WrapperNeat
		{
			get { return wrapper.ToString().Replace("\r\n\r\n", "\r\n"); }
		}
		readonly DropOffWrapper wrapper;
	}
}
