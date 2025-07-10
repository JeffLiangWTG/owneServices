using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusInBondPerson : Customs.Business.CusInBondPerson
	{
		public CusInBondPerson(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static readonly CusInBondPersonTypeDecider TypeDecider = new CusInBondPersonTypeDecider();

		[RelatedBusinessObject(nameof(NctsHeader))]
		public override ZGuid CP_BH_Header
		{
			get => base.CP_BH_Header;
			set => base.CP_BH_Header = value;
		}

		public NctsHeader NctsHeader => Factory.Load<NctsHeader>(CP_BH_Header);

		public const string LocationContactType = "LOC";

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new CusInBondPersonUniqueIndexFailureHandler(this)); }
		}
		IUniqueIndexFailureHandler uniqueIndexFailureHandler;
	}
}
