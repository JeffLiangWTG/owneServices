using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.UCC5.Testing
{
	sealed class IM460ProviderTest : TestCase
	{
		public void TestMovementReferenceNumber()
		{
			AssertEquals("12MRN345CDEFG678R9", provider.MovementReferenceNumber);
		}

		public void TestControlNotificationDate()
		{
			AssertEquals(new ZDateTime(2024, 02, 20, 23, 59, 00), provider.ControlNotificationDate);
		}

		public void TestTimeLimitForControl()
		{
			AssertEquals(new ZDateTime(2024, 03, 07, 14, 37, 00), provider.TimeLimitForControl);
		}

		public void TestCustomsOfficeLodgement()
		{
			AssertEquals("OF123456", provider.CustomsOfficeLodgement);
		}

		public void TestOverallControlTypeCoded()
		{
			AssertEquals("O", provider.OverallControlTypeCoded);
		}

		public void TestControlTypesCoded()
		{
			AssertEquals(2, provider.ControlTypes.Count);
			var types = provider.ControlTypes.ToArray();
			AssertEquals("O", types[0].ControlTypeCoded);
			AssertEquals("Revenue", types[0].ControlTypeAgency);
			AssertEquals("1", types[1].ControlTypeCoded);
			AssertEquals("Revenue1", types[1].ControlTypeAgency);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new IM460Provider(new Im460
			{
				Declaration = new DeclarationType()
				{
					Mrn = "12MRN345CDEFG678R9",
					ControlNotificationDate = "202402202359",
					TimeLimitForControl = "202403071437",
					CustomsOffices = new DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "OF123456",
					}
				},
				OverallControlType = new OverAllControlsType()
				{
					ControlTypeCoded = "O"
				},
				GoodsShipment = new System.Collections.ObjectModel.Collection<GoodsShipmentItemType>()
				{
					new GoodsShipmentItemType()
					{
						GoodsItemNumber16 = "1",
						ControlType = new System.Collections.ObjectModel.Collection<ControlsType>() {
							new ControlsType()
							{
								ControlTypeCoded = "O",
								ControlAgency = "Revenue",
							}
						}
					},
					new GoodsShipmentItemType()
					{
						GoodsItemNumber16 = "2",
						ControlType = new System.Collections.ObjectModel.Collection<ControlsType>() {
							new ControlsType()
							{
								ControlTypeCoded = "O",
								ControlAgency = "Revenue",
							},
							new ControlsType()
							{
								ControlTypeCoded = "1",
								ControlAgency = "Revenue1",
							}
						}
					}
				}
			});
		}
		IM460Provider provider;
	}
}
