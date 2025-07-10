using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public static class Extensions
	{
		public static ZString GetOfficeReferenceNumber(this EMCSJobDeclaration emcs, ZString type) => emcs?.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code == type)?.CY_Data ?? ZString.Empty;

		public static DateTime? ToNullableDateTime(this ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToDateTime() : null;
	}
}
