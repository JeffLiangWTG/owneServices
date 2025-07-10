using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.Base.Testing
{
	public abstract class MultipleReversingBaseFormTest : AccountingZFormBasherTest
	{
		#region TestMakeRequiredFieldsEditableForReversingOnBizObject

		public void TestMakeRequiredFieldsEditableForReversingOnBizObject()
		{
			AssertMakeRequiredFieldsEditableForReversingOnBizObject(false);
		}

		public void TestMakeRequiredFieldsEditableForReversingOnBizObject_WithRowError()
		{
			AssertMakeRequiredFieldsEditableForReversingOnBizObject(true);
		}

		void AssertMakeRequiredFieldsEditableForReversingOnBizObject(bool rowErrorCase)
		{
			var multipleReversingProvider = GetNewMultipleReversingProvider();
			using (var form = GetNewForm(multipleReversingProvider))
			{
				form.DisplayMode = ODisplayMode.Delete;

				var reversedBizoWrapper = GetNewAlreadyReversedBusinessObjectWrappedForBinding();
				AddReversedBizo(reversedBizoWrapper);
				AssertReadOnly("form.Show", reversedBizoWrapper, () => form.Show());

				reversedBizoWrapper = GetNewAlreadyReversedBusinessObjectWrappedForBinding();
				AssertReadOnly("AddReversedBizo", reversedBizoWrapper, () => AddReversedBizo(reversedBizoWrapper));
			}

			void AddReversedBizo(NonPersistentBusinessObject reversedBizoWrapper) => multipleReversingProvider.BizObjectsAlreadyReversed.Add(reversedBizoWrapper);

			void AssertReadOnly(string message, NonPersistentBusinessObject reversedBizoWrapper, Action testAction)
			{
				multipleReversingProvider.SetReadOnlyIncludingChildren(true); // this is what ZContrioller does in SwitchToDeleteForm called in ShowDeleteForm for each transaction added to multi reversing form

				var reversedBizo = multipleReversingProvider.GetWrappedBusinessEntity(reversedBizoWrapper);
				if (rowErrorCase)
				{
					reversedBizo.AddRowError("test");
				}
				else
				{
					AssertNoRowErrors(message, reversedBizo);
				}

				testAction();

				if (rowErrorCase)
				{
					Assert(message + " bizo.ReadOnly", reversedBizo.ReadOnly);
				}
				else
				{
					Assert(message + " bizo.ReadOnly", !reversedBizo.ReadOnly);
					AssertNotReadonlyWhenNoRowErrors(message, reversedBizoWrapper);
				}

				Assert(message, !multipleReversingProvider.ReadOnly);
			}
		}

		protected abstract void AssertNotReadonlyWhenNoRowErrors(string message, NonPersistentBusinessObject wrapper);

		#endregion

		protected abstract NonPersistentBusinessObject GetNewAlreadyReversedBusinessObjectWrappedForBinding();

		protected abstract BusinessObject GetOriginalTransaction(BusinessObject reversedBizo);

		protected abstract MultipleReversingProviderBase GetNewMultipleReversingProvider();

		protected abstract MultipleReversingBaseForm GetNewForm(MultipleReversingProviderBase multipleReversingProvider);

		protected override Form GetFormToBashCore()
		{
			var multipleReversingProvider = GetNewMultipleReversingProvider();
			var wrapper = GetNewAlreadyReversedBusinessObjectWrappedForBinding();
			var reversedBizo = multipleReversingProvider.GetWrappedBusinessEntity(wrapper);
			multipleReversingProvider.BizObjectsForReversing.Add(GetOriginalTransaction(reversedBizo));

			multipleReversingProvider.BizObjectsAlreadyReversed.Add(wrapper);

			((IBusinessObjectState)multipleReversingProvider).ClearHasChangesIncludingChildren();

			return GetNewForm(multipleReversingProvider);
		}
	}
}
