using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class PackingGroup : BasePackingGroup, Integration.Customs.EU.IPackingGroup
	{
		public PackingGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString GetConvertedPackType(ZString packType)
		{
			return packType.IsEmpty ? packType : Converter.GetTwoCharacterUnitType(packType).MappedCode;
		}

		InvoiceLineFromOrderLineSynchroniser converter;
		InvoiceLineFromOrderLineSynchroniser Converter => converter ?? (converter = new InvoiceLineFromOrderLineSynchroniser());

		public new static readonly PackingGroupTypeDecider TypeDecider = new PackingGroupTypeDecider();
	}
}
