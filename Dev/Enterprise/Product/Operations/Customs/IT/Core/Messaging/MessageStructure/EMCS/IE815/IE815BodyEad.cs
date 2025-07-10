using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815BodyEad
{
	public IE815BodyEad(IIE815BodyEad bodyEad)
	{
		this.bodyEad = bodyEad;
	}
	readonly IIE815BodyEad bodyEad;

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("R")]
	public ZInt BodyRecordUniqueReference => bodyEad.BodyRecordUniqueReference;

	[MessageLayout(Order = 1)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, true)]
	[MessageFieldRules("R", "R047", "R048", "R070", "R072")]
	public ZString ExciseProductCode => bodyEad.ExciseProductCode;

	[MessageLayout(Order = 2)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 8, false)]
	[MessageFieldRules("R", "R019")]
	public ZString CnCode => bodyEad.CnCode;

	[MessageLayout(Order = 3)]
	[MessageFieldDecimalRepresentation(12, 3, false)]
	[MessageFieldRules("R", "R021")]
	public ZDecimal Quantity => bodyEad.Quantity;

	[MessageLayout(Order = 4)]
	[MessageFieldDecimalRepresentation(13, 2, false)]
	[MessageFieldRules("R", "R022")]
	public ZDecimal GrossWeight => bodyEad.GrossWeight;

	[MessageLayout(Order = 5)]
	[MessageFieldDecimalRepresentation(13, 2, false)]
	[MessageFieldRules("R", "R023")]
	public ZDecimal NetWeight => bodyEad.NetWeight;

	[MessageLayout(Order = 6)]
	[MessageFieldDecimalRepresentation(3, 2, false)]
	[MessageFieldRules("C", "C027")]
	public ZDecimal Density => bodyEad.Density;

	[MessageLayout(Order = 7)]
	[MessageFieldDecimalRepresentation(3, 2, false)]
	[MessageFieldRules("C", "C026")]
	public ZDecimal AlcoholicStrength => bodyEad.AlcoholicStrength;

	[MessageLayout(Order = 8)]
	[MessageFieldDecimalRepresentation(3, 2, false)]
	[MessageFieldRules("C", "C063")]
	public ZDecimal DegreePlato => bodyEad.DegreePlato;

	[MessageLayout(Order = 9)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 350, false)]
	[MessageFieldRules("O")]
	public ZString FiscalMark => bodyEad.FiscalMark;

	[MessageLayout(Order = 10)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C012")]
	public ZString FiscalMarkLanguage => bodyEad.FiscalMarkLanguage;

	[MessageLayout(Order = 11)]
	[MessageFieldBoolRepresentation()]
	[MessageFieldRules("C", "D005")]
	public ZBool FiscalMarkUsedFlag => bodyEad.FiscalMarkUsedFlag;

	[MessageLayout(Order = 12)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 350, false)]
	[MessageFieldRules("O")]
	public ZString DesignationOfOrigin => bodyEad.DesignationOfOrigin;

	[MessageLayout(Order = 13)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C012")]
	public ZString DesignationOfOriginLanguage => bodyEad.DesignationOfOriginLanguage;

	[MessageLayout(Order = 14)]
	[MessageFieldIntegerRepresentation(15, false)]
	[MessageFieldRules("O", "R024")]
	public ZInt SizeOfProducer => bodyEad.SizeOfProducer;

	[MessageLayout(Order = 15)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldRules("O", "R020")]
	public ZInt TaricCode => bodyEad.TaricCode;

	[MessageLayout(Order = 16)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 4, false)]
	[MessageFieldRules("O", "R020")]
	public ZString CaddCode => bodyEad.CaddCode;

	[MessageLayout(Order = 17)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 5, false)]
	[MessageFieldRules("C", "C059")]
	public ZString AamsCode => bodyEad.AamsCode;

	[MessageLayout(Order = 18)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 339, false)]
	[MessageFieldRules("C", "D007")]
	public ZString CommercialDescription => bodyEad.CommercialDescription;

	[MessageLayout(Order = 19)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C012")]
	public ZString CommercialDescriptionLanguage => bodyEad.CommercialDescriptionLanguage;

	[MessageLayout(Order = 20)]
	[MessageFieldStringRepresentation(CharType.Alphanumeric, 350, false)]
	[MessageFieldRules("C", "D008")]
	public ZString BrandNameOfProducts => bodyEad.BrandNameOfProducts;

	[MessageLayout(Order = 21)]
	[MessageFieldStringRepresentation(CharType.Alphabetical, 2, false)]
	[MessageFieldRules("C", "C012")]
	public ZString BrandNameOfProductsLanguage => bodyEad.BrandNameOfProductsLanguage;

	[MessageLayout(Order = 22)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("R")]
	public ZInt TotalPackagesIterations => bodyEad.Packages.Count();

	[MessageLayout(Order = 23)]
	public IEnumerable<IE815Package> Packages
	{
		get
		{
			var i = 1;
			foreach (var package in bodyEad.Packages)
			{
				yield return new IE815Package(package, i++);
			}
		}
	}

	[MessageLayout(Order = 24)]
	public IE815WineProduct WineProduct => new IE815WineProduct(bodyEad.WineProduct);
}
