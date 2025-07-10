using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FreeWaitingTime))]
	sealed class FreeWaitingTimeTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new FreeWaitingTime();

			result.CFS = new ZDateTime(2005, 1, 2, 12, 13, 14);
			result.CTO = new ZDateTime(2005, 2, 3, 13, 14, 15);
			result.CYD = new ZDateTime(2005, 3, 4, 14, 15, 16);
			result.CNR = new ZDateTime(2005, 5, 6, 15, 16, 17);
			result.Other = new ZDateTime(2005, 7, 8, 16, 17, 18);

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion

		#region TestValidation

		public void TestValidation()
		{
			var item = new FreeWaitingTime();

			item.DropMode = "";
			AssertHasError(item.DropModeInfo, "Please enter a value.");

			item.DropMode = "ZZZ";
			AssertHasError(item.DropModeInfo, "Enter a valid selection.");

			item.DropMode = "ANY";
			AssertNoErrors(item.DropModeInfo);

			item.CNTType = ZGuid.Missing;
			AssertHasError(item.CNTTypeInfo, "Please enter a valid Container Type.");

			item.CNTType = item.ContainerTypes.Cast<BusinessObject>().First().PK;
			AssertNoErrors(item.CNTTypeInfo);

			TestDateTimeValidation(item.CNEInfo, item);
			TestDateTimeValidation(item.CNRInfo, item);
			TestDateTimeValidation(item.CYDInfo, item);
			TestDateTimeValidation(item.CFSInfo, item);
			TestDateTimeValidation(item.CTOInfo, item);
			TestDateTimeValidation(item.OtherInfo, item);
		}

		void TestDateTimeValidation(ZPropertyInfo info, FreeWaitingTime item)
		{
			info.Value = ZDateTime.Invalid;
			AssertHasError(info, "Please enter a valid time.");

			info.Value = ZDateTime.Now;
			AssertNoErrors(info);
		}

		#endregion
	}
}
