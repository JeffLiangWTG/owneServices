using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusInBondPersonTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type bizOType = null;
			var headerPK = row != null ? new ZGuid(row[AutoCusInBondPerson.Schema.CP_BH_Header]) : ZGuid.Invalid;
			var nctsHeader = (headerPK.IsValid ? factory.Load<NctsHeader>(headerPK) : null) as ICusInBondPersonTypeProvider;
			if (nctsHeader != null)
			{
				bizOType = nctsHeader.CusInBondPersonType;
			}
			return bizOType;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}

	public interface ICusInBondPersonTypeProvider
	{
		Type CusInBondPersonType { get; }
	}
}

