using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	[RemovalType(RemovalCode)]
	public class Fallback : CusUnderbond, Integration.Customs.GB.CCSUK.ICusUnderbond_Fallback
	{
		public Fallback(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override Customs.Business.CusUnderbondValidation GetNewValidation()
		{
			return new FallbackValidation(this);
		}

		public const string RemovalCode = "FBK";

		public override ZString RemovalTypeHuman
		{
			get { return "Fallback"; }
		}

		public override string ToString()
		{
			var table = new HtmlTableCreator();
			WriteCorePropertiesForInterpretation(table);
			table.WriteRow("Reference", AgentsReference);
			return table.ToHtml();
		}

		public override ZString Description
		{
			get { return "Fallback"; }
		}

		protected override void DefaultPackagesFromParent()
		{ }  // do not set packages.  FBK has only this one field that is user-ediable, and if we set it for them, they do not, and an uncommittted row is never committed to the grid.   Pff.  Make user enter it, which commits the row to the grid. 

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			C4_ApplicationCode = Enterprise.Customs.Business.CusUnderbondApplicationCodeList.Codes.GBFallback;
		}
	}
}
