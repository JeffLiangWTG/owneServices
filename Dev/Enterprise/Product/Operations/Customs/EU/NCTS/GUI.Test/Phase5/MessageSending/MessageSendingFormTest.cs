using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : MessageSendingFormWithValidationDetailsAbstractTest
	{
		protected override Form GetFormToBashCore() => new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(header));

		[RequiresSTA]
		public void TestAdditionalColumnVisibility()
		{
			var parent = new NctsHeaderMessageSendingObjectParent(header);
			using (var form = new MessageSendingForm(parent))
			{
				var messageSendingObjectsGrid = form.FindSingle<ZGrid>();
				form.Show();
				AssertSequencesEqual("Columns", new[]
				{
					NctsHeaderMessageSendingObject.Schema.ShouldSend,
					AutoNctsHeaderMessageSendingObject.Schema.LRN,
					AutoNctsHeaderMessageSendingObject.Schema.MRN,
					AutoNctsHeaderMessageSendingObject.Schema.MessageType,
					AutoNctsHeaderMessageSendingObject.Schema.Justification,
				},
				messageSendingObjectsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestAvailableColumns()
		{
			using (var form = new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(header)))
			{
				var messageSendingObjectsGrid = form.FindSingle<ZGrid>();
				form.Show();
				AssertSequencesEqual("Columns", new[]
				{
					NctsHeaderMessageSendingObject.Schema.ShouldSend,
					AutoNctsHeaderMessageSendingObject.Schema.LRN,
					AutoNctsHeaderMessageSendingObject.Schema.MRN,
					AutoNctsHeaderMessageSendingObject.Schema.MessageType,
					AutoNctsHeaderMessageSendingObject.Schema.Justification,
				},
				messageSendingObjectsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		[RequiresSTA]
		public void TestColumnsWidth()
		{
			using (var form = new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(header)))
			{
				var messageSendingObjectsGrid = form.FindSingle<ZGrid>();
				CombineAssertions(() =>
				{
					AssertEquals(nameof(NctsHeaderMessageSendingObject.Schema.ShouldSend), 40, messageSendingObjectsGrid.GetColumnStyle(NctsHeaderMessageSendingObject.Schema.ShouldSend).Width);
					AssertEquals(nameof(AutoNctsHeaderMessageSendingObject.Schema.LRN), 160, messageSendingObjectsGrid.GetColumnStyle(AutoNctsHeaderMessageSendingObject.Schema.LRN).Width);
					AssertEquals(nameof(AutoNctsHeaderMessageSendingObject.Schema.MRN), 160, messageSendingObjectsGrid.GetColumnStyle(AutoNctsHeaderMessageSendingObject.Schema.MRN).Width);
					AssertEquals(nameof(AutoNctsHeaderMessageSendingObject.Schema.MessageType), 100, messageSendingObjectsGrid.GetColumnStyle(AutoNctsHeaderMessageSendingObject.Schema.MessageType).Width);
					AssertEquals(nameof(AutoNctsHeaderMessageSendingObject.Schema.Justification), 100, messageSendingObjectsGrid.GetColumnStyle(AutoNctsHeaderMessageSendingObject.Schema.Justification).Width);
				});
			}
		}

		[RequiresSTA]
		public void TestReleaseRequestColumn()
		{
			var mockConfiguration = new Mock<MessageSendingConfiguration>() { CallBase = true };
			var list = new CodeDescriptionPairList();
			list.AddPair(NCTS5DeparturePhaseList.Codes.ReleaseRequest, NCTS5DeparturePhaseList.Descriptions.ReleaseRequest);
			mockConfiguration.Setup(x => x.MessageTypeList(It.IsAny<NctsHeader>())).Returns(list);
			using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, mockConfiguration.Object))
			{
				using (var form = new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(header)))
				{
					var messageSendingObjectsGrid = form.FindSingle<ZGrid>();
					CombineAssertions(() =>
					{
						var releaseRequestColumnStyle = messageSendingObjectsGrid.GetColumnStyle(NctsHeaderMessageSendingObject.Schema.ReleaseRequest);
						AssertEquals("releaseRequestColumnStyle.Width", 100, releaseRequestColumnStyle.Width);
						AssertEquals("releaseRequestColumnStyle.IsMandatory", true, releaseRequestColumnStyle.IsMandatory);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestPreviewMessageCheckboxVisible()
		{
			GlbStaff.CurrentUser.GS_IsDeveloper = false;
			using (var form = new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(header)))
			{
				var previewMessageCheckBox = form.PreviewMessageCheckBox;
				form.Show();
				Assert("Checkbox should be hidden for non-developer user", !previewMessageCheckBox.Visible);
			}

			GlbStaff.CurrentUser.GS_IsDeveloper = true;
			using (var form = new MessageSendingForm(new NctsHeaderMessageSendingObjectParent(header)))
			{
				var previewMessageCheckBox = form.PreviewMessageCheckBox;
				form.Show();
				Assert("Checkbox should be visible for developer user", previewMessageCheckBox.Visible);
			}
		}

		public void TestSendWithAdditionalWarningCheckBox_Visible_ShouldHideSendWithAdditionalWarningCheckBoxIsTrue()
		{
			AssertSendWithAdditionalWarningCheckBox_Visible(true);
		}

		public void TestSendWithAdditionalWarningCheckBox_Visible_ShouldHideSendWithAdditionalWarningCheckBoxIsFalse()
		{
			AssertSendWithAdditionalWarningCheckBox_Visible(false);
		}

		public void TestMessageSendingGridColumnLayoutProvider_IsUsedFromNctsPhase5LayoutProvider()
		{
			var gridColumnLayoutMock = new Mock<IGridColumnLayout>();
			gridColumnLayoutMock.Setup(g => g.Columns).Returns(new[] { new ZTextBoxColumnStyleInfo(AutoNctsHeaderMessageSendingObject.Schema.LRN, 100) });

			var dummyMessageSendingGridColumnLayoutProvider = new Mock<IGridColumnLayoutProvider>();
			dummyMessageSendingGridColumnLayoutProvider.Setup(p => p.Layout).Returns(gridColumnLayoutMock.Object);

			var dummyNctsPhase5LayoutProvider = new Mock<INctsPhase5LayoutProvider>();
			dummyNctsPhase5LayoutProvider.Setup(m => m.GetMessageSendingGridColumnLayout(It.IsAny<BaseMessageSendingObjectParent>())).Returns(dummyMessageSendingGridColumnLayoutProvider.Object);

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(o => o.GetObject()).Returns(dummyNctsPhase5LayoutProvider.Object);

			try
			{
				var dummyProviderHashTable = new Hashtable { { Core.Constants.CountryCodes.Ireland, objectHandleMock.Object } };
				ObjectFactory.Substitute("NctsPhase5LayoutProviders", dummyProviderHashTable);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
				{
					header = CreateHeader();
					header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
					var parent = new NctsHeaderMessageSendingObjectParent(header);

					using var form = new MessageSendingFormForTest(parent);
					CombineAssertions("When NCTS5+IE having Mock MessageSendingGridColumnLayoutProvider", () =>
					{
						AssertNotNull("MessageSendingGridColumnLayoutProvider", form.MessageSendingGridColumnLayoutProvider_Exposed);
						AssertSame("MessageSendingGridColumnLayoutProvider Object", dummyMessageSendingGridColumnLayoutProvider.Object, form.MessageSendingGridColumnLayoutProvider_Exposed);
					});

					header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
					AssertNull("MessageSendingGridColumnLayoutProvider for NCTS44+IE", form.MessageSendingGridColumnLayoutProvider_Exposed);
				}
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
				{
					header = CreateHeader();
					var parent = new NctsHeaderMessageSendingObjectParent(header);
					using var form = new MessageSendingFormForTest(parent);
					AssertNull("MessageSendingGridColumnLayoutProvider for NCTS5+IT with no value", form.MessageSendingGridColumnLayoutProvider_Exposed);
				}
			}
			finally
			{
				ObjectFactory.DisposeSubstitutions();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = CreateHeader();
		}

		NctsHeader header;

		NctsHeader CreateHeader()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_HeaderType = NctsMovementType.Codes.Departure;
			return header;
		}

		void AssertSendWithAdditionalWarningCheckBox_Visible(bool shouldHideSendWithAdditionalWarningCheckBox)
		{
			var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
			goodsItem.BY_GrossWeightUnit = "KG";
			goodsItem.BY_GrossWeight = 10;
			goodsItem.BY_NetWeightUnit = "KG";
			goodsItem.BY_NetWeight = 20;

			var propertyDic = new Dictionary<string, bool>
			{
				{ "ShouldFillAdditionalWarningsOnSendScreenCore", true },
				{ "ShouldHideSendWithAdditionalWarningCheckBoxCore", shouldHideSendWithAdditionalWarningCheckBox
			} };
			var messageSendingObjectParent = new NctsHeaderMessageSendingObjectParent(header);

			CombineAssertions(() =>
			{
				using (var form = new MessageSendingForm(messageSendingObjectParent))
				using (NctsConfigurationTestHelper.TemporarilySetMessageSendingConfiguration(Factory, propertyDic))
				{
					using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
					{
						goodsItem.Validation.ValidateBY_NetWeight();
						AssertEquals("Precondition", expected: false, messageSendingObjectParent.AdditionalWarnings.IsEmpty);

						form.Show();
						var sendAdditionalWarningCheckBox = form.FindSingle<ZCheckBox>("SendWithAdditionalWarningCheckBox");
						AssertEquals("SendWithAdditionalWarningCheckBox.Visible", expected: !shouldHideSendWithAdditionalWarningCheckBox, sendAdditionalWarningCheckBox.Visible);
					}
				}
			});
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive) => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isTransitionPeriodActive);
	}

	sealed class MessageSendingFormForTest : MessageSendingForm
	{
		public MessageSendingFormForTest(NctsHeaderMessageSendingObjectParent sendingObjectParent) : base(sendingObjectParent)
		{
		}

		public IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider_Exposed => base.MessageSendingGridColumnLayoutProvider;
	}
}
