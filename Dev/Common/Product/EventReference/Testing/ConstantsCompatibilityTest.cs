using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(Enterprise.ZArchitecture.Business.Events))]

namespace CargoWise.EventReference.Testing
{
	sealed class ConstantsCompatibilityTest : TestCase
	{
		public void TestServiceTypes_Codes()
		{
			Assert("ServiceTypes.Code is out of sync",
				ConstantsAreEqual(typeof(Constants.ServiceTypes.Code), typeof(Enterprise.Core.Constants.FreightServiceType.Codes)));
		}

		public void TestEventCodes()
		{
			AssertEquals("ReleaseRequestedCode", Constants.EventCodes.ReleaseRequestedCode, Enterprise.ZArchitecture.Business.AutoEvents.ReleaseRequestedCode);
		}

		#region Implementation

		bool ConstantsAreEqual(Type const1, Type const2)
		{
			var const1Values = GetValues(const1);
			var const2Values = GetValues(const2);

			return !const1Values.Except(const2Values).Any();
		}

		IEnumerable<string> GetValues(Type constants)
		{
			return constants
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
				.Select(f => Convert.ToString(f.GetValue(null)));
		}

		#endregion
	}
}