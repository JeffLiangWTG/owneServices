using System.Collections.Generic;
using Enterprise.DataTransfer.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class PkgUnitXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		protected PkgUnitXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(Core.Constants.PkgUnit.Bag, "BAG");
			yield return new Mapping(Core.Constants.PkgUnit.BulkBag, "BBG");
			yield return new Mapping(Core.Constants.PkgUnit.BreakBulk, "BBK");
			yield return new Mapping(Core.Constants.PkgUnit.BaleCompressed, "BLC");
			yield return new Mapping(Core.Constants.PkgUnit.BaleUncompressed, "BLU");
			yield return new Mapping(Core.Constants.PkgUnit.Bundle, "BND");
			yield return new Mapping(Core.Constants.PkgUnit.Bottle, "BOT");
			yield return new Mapping(Core.Constants.PkgUnit.Box, "BOX");
			yield return new Mapping(Core.Constants.PkgUnit.Basket, "BSK");
			yield return new Mapping(Core.Constants.PkgUnit.Case, "CAS");
			yield return new Mapping(Core.Constants.PkgUnit.Container, "CNT");
			yield return new Mapping(Core.Constants.PkgUnit.Coil, "COI");
			yield return new Mapping(Core.Constants.PkgUnit.Cradle, "CRD");
			yield return new Mapping(Core.Constants.PkgUnit.Crate, "CRT");
			yield return new Mapping(Core.Constants.PkgUnit.Carton, "CTN");
			yield return new Mapping(Core.Constants.PkgUnit.Cylinder, "CYL");
			yield return new Mapping(Core.Constants.PkgUnit.Dozen, "DOZ");
			yield return new Mapping(Core.Constants.PkgUnit.Drum, "DRM");
			yield return new Mapping(Core.Constants.PkgUnit.Envelope, "ENV");
			yield return new Mapping(Core.Constants.PkgUnit.Gross, "GRS");
			yield return new Mapping(Core.Constants.PkgUnit.Keg, "KEG");
			yield return new Mapping(Core.Constants.PkgUnit.Mix, "MIX");
			yield return new Mapping(Core.Constants.PkgUnit.Pail, "PAI");
			yield return new Mapping(Core.Constants.PkgUnit.Piece, "PCE");
			yield return new Mapping(Core.Constants.PkgUnit.Package, "PKG");
			yield return new Mapping(Core.Constants.PkgUnit.Pallet, "PLT");
			yield return new Mapping(Core.Constants.PkgUnit.Reel, "REL");
			yield return new Mapping(Core.Constants.PkgUnit.Roll, "RLL");
			yield return new Mapping(Core.Constants.PkgUnit.RollOnRollOff, "ROR");
			yield return new Mapping(Core.Constants.PkgUnit.Sheet, "SHT");
			yield return new Mapping(Core.Constants.PkgUnit.Skid, "SKD");
			yield return new Mapping(Core.Constants.PkgUnit.Spool, "SPL");
			yield return new Mapping(Core.Constants.PkgUnit.Tote, "TOT");
			yield return new Mapping(Core.Constants.PkgUnit.Tube, "TUB");
			yield return new Mapping(Core.Constants.PkgUnit.Unit, "UNT");
		}

		public static readonly PkgUnitXmlCodeMappings Instance = new PkgUnitXmlCodeMappings();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string Name
		{
			get { return "Package Unit"; }
		}
	}
}
