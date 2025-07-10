using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[Serializable]
	class NctsDeclarationLockLogSubscriber : DeclarationLockLogSubscriber
	{
		public override string Name => "NctsDeclarationLockLogSubscriber";

		public override string FriendlyName => (NoResString)"NCTS Declaration Lock Log Subscriber";

		public override string[] TableNames => new[] { CusInBondHeader.Schema.TableName, CusInBondMoveHeader.Schema.TableName };

		protected override ICustomsFileParent GetLockParent(BusinessObjectFactory factory, ZString parentTable, ZGuid parentPk) =>
			parentTable.ToString() switch
			{
				CusInBondHeaderSchema.Constants.Prefix => factory.Load<CusInBondHeader>(parentPk) as NctsHeader,
				CusInBondMoveHeaderSchema.Constants.Prefix => factory.Load<CusInBondMoveHeader>(parentPk)?.Header as NctsHeader,
				_ => base.GetLockParent(factory, parentTable, parentPk)
			};
	}
}
