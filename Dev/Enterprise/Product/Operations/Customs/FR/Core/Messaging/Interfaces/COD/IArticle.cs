using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.COD
{
	public interface IArticle
	{
		ZString EntryNumber { get; } //Articles/Article/refdec
		ZString Direction { get; } //Articles/Article/typflux
		ZString ItemNumber { get; } //Articles/Article/numart
		IEnumerable<IDocAapurer> Documents { get; } //Articles/Article/Documents
		IApur Apur { get; } //Articles/Article/Apur
		IEnumerable<ITaxeAapurer> Taxes { get; } //Articles/Article/Taxes
	}
}
