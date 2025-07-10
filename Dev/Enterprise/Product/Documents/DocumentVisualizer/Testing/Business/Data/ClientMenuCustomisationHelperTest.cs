using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class ClientMenuCustomisationHelperTest : TransactionedTestCase
	{
		public void TestExcludedGetExcludedTemplatePKs_None()
		{
			var excludedTemplatePKs = ClientMenuCustomisationHelper.GetExcludedTemplatePKs();

			AssertContainsExactElementsInAnyOrder("By default all Yusen and DHL templates are excluded",
				YusenTemplatePKs.Concat(DHLTemplatePKs), excludedTemplatePKs);
		}

		public void TestExcludedGetExcludedTemplatePKs_YusenTemplatesAreInUse()
		{
			var overrides = new AdditionalHouseBillOfLadingTypeCollection();
			var yusen = new AdditionalHouseBillOfLadingType
			{
				Code = Enterprise.Core.Constants.AddtionalHouseBillTypeMenu.Code.YusenHBL,
				Description = (NoResString)Enterprise.Core.Constants.AddtionalHouseBillTypeMenu.YusenHBLMenuName
			};
			overrides.Add(yusen);

			yusen.Enable = true;

			using (FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, overrides))
			{
				var excludedTemplatePKs = ClientMenuCustomisationHelper.GetExcludedTemplatePKs();

				AssertContainsExactElementsInAnyOrder("When Yusen is active then DHL templates are excluded", DHLTemplatePKs, excludedTemplatePKs);
			}
		}

		public void TestExcludedGetExcludedTemplatePKs_DHLTemplatesAreInUse()
		{
			var overrides = new AdditionalHouseBillOfLadingTypeCollection();
			var dhl = new AdditionalHouseBillOfLadingType
			{
				Code = Enterprise.Core.Constants.AddtionalHouseBillTypeMenu.Code.DHLHBL,
				Description = (NoResString)Enterprise.Core.Constants.AddtionalHouseBillTypeMenu.DHLHBLMenuName
			};
			overrides.Add(dhl);

			dhl.Enable = true;

			using (FreightDataRegistry.Instance.AddtionalHouseBillOfLadingTypes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, overrides))
			{
				var excludedTemplatePKs = ClientMenuCustomisationHelper.GetExcludedTemplatePKs();

				AssertContainsExactElementsInAnyOrder("When DHL is active then Yusen templates are excluded", YusenTemplatePKs, excludedTemplatePKs);
			}
		}

		readonly ZGuid[] YusenTemplatePKs = new[]
		{
			new ZGuid("339C7E0D-2EAA-43F8-AAE6-0F2A4F347C63"),
			new ZGuid("F5AF8285-18EB-43BC-A27F-18A5B953FD9D"),
			new ZGuid("86D8794E-7841-4F3F-93EC-1C8675A377AE"),
			new ZGuid("24CEFD9B-9810-405B-9616-9C97A855B801"),
			new ZGuid("EDB34F00-6DB9-401F-92F4-CE3626C11BF5"),
			new ZGuid("FDE94F5A-84AF-415A-B611-E7573D166154")
		};

		readonly ZGuid[] DHLTemplatePKs = new[]
		{
			new ZGuid("40052CD3-C07C-4CB9-88FF-1EB02B286B3B"),
			new ZGuid("6A64B689-ABE7-4D2A-B944-5BCD597B6FAF"),
			new ZGuid("785A2FB7-F6C1-473D-9F28-80812D179A00"),
			new ZGuid("339C7E0D-2EAA-43F8-AAE6-0F2A4F347C63"),
			new ZGuid("FEAC7D6F-8323-4135-AD5E-D08C9CBA2DE6"),
			new ZGuid("EDEE7623-2C97-4271-A844-DFF3D64FCA75")
		};
	}
}
