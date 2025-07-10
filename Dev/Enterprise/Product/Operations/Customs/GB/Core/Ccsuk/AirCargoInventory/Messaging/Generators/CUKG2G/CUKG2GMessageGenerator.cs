using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Edifact;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Generators
{
	public class CUKG2GMessageGenerator : CusAwbToInventoryMessageGenerator
	{
		public CUKG2GMessageGenerator(CustomsExportConsolIntegrationWrapper consolWrapper, UNCharacterSet charSet, ErrorCollector errorCollector)
			: base(consolWrapper.Factory)
		{
			this.charSet = charSet;
			this.errorCollector = errorCollector;
			iG2gHeader = new G2gHeaderFromConsol(consolWrapper);
			creator = new CUKG2GCreator(iG2gHeader, errorCollector);
		}

		public override string MakeMessageText()
		{
			return creator.MakeMessageText(charSet);
		}

		public override ZString MessageInterpretation
		{
			get { return new CUKG2GMessageInterpreter(iG2gHeader).MakePretty(); }
		}

		readonly CUKG2GCreator creator;
		readonly UNCharacterSet charSet;
		readonly IG2gHeader iG2gHeader;
		protected ErrorCollector errorCollector;
	}
}
