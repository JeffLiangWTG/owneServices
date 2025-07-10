using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Provides extension methods for BindingContext.
	/// </summary>
	public static class BindingContextExtensions
	{
		/// <summary>
		/// Get the BindingManagerBase for the meta-data of a PropertyDescriptor. Null is returned if
		/// the property doesn't support that type of meta data.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "metaData")]
		public static BindingManagerBase GetBindingManager(this BindingContext bc, object dataSource, string dataMember, string metaDataType)
		{
			BindingMemberInfo member = new BindingMemberInfo(dataMember);
			BindingManagerBase bm = EnsureListManager(bc, dataSource, member.BindingPath);
			KPropertyDescriptor property = bm.GetItemProperties()[member.BindingField] as KPropertyDescriptor;

			PropertyDescriptor metaDataProperty = null;
			if (property != null)
			{
				metaDataProperty = MetaData.GetMetaDataProperty(property.ComponentType, property, metaDataType);
			}
			if (metaDataProperty == null)
			{
				return null;
			}
			else
			{
				return EnsureListManager(bc, dataSource, new KBindingMemberInfo(member.BindingPath, metaDataProperty.Name).BindingMember);
			}
		}

		public static void SetBindingManagerCurrent(this BindingContext bc, IDataBoundControl control, string path, object newCurrent)
		{
			BindingManagerBase bm = EnsureListManager(bc, control.DataSource, new KBindingMemberInfo(control.DataMember, path).BindingMember);
			CurrencyManager cm = bm as CurrencyManager
				?? throw new ArgumentException("Expected a list not a property");
			int index = cm.List.IndexOf(newCurrent);
			if (index != -1)
			{
				cm.Position = index;
			}
		}

		#region EnsureListManager

		/// <summary>
		/// You should call this method before using this[object, string] to ensure
		/// - CurrencyManager->RelatedPropertyManager configuration doesn't throw an
		///   IndexOutOfBoundsException
		/// - CurrencyManager and PropertyManager objects are configured with NTier enhancements.
		/// </summary>
		public static BindingManagerBase EnsureListManager(this BindingContext bc, object dataSource, string dataMember)
		{
			BindingManagerBase result = null;
			BindingManagerBase parent = null;
			if (string.IsNullOrEmpty(dataMember) || bc.Contains(dataSource, dataMember))
			{
				result = GetBindingContextEnsureListManagerSafe(bc, dataSource, dataMember);
			}
			else
			{
				parent = EnsureListManager(bc, dataSource, new KBindingMemberInfo(dataMember).BindingPath);
				CurrencyManager parentCM = parent as CurrencyManager;
				if (parentCM == null)
				{
					result = GetBindingContextEnsureListManagerSafe(bc, dataSource, dataMember);
				}
				else
				{
					ICollectionAlwaysReturnElementsForBinding parentCMList = (parentCM == null) ? null : parentCM.List as ICollectionAlwaysReturnElementsForBinding;
					using (parentCMList == null ? DisposableAction.NoAction : parentCMList.AlwaysReturnElementsForBinding())
					{
						// This occurs in JOTOrganisationFormTest.BashingForm. This isn't able to be reproduced elsewhere.
						if (parentCM.Count > 0 && parentCM.Position == -1)
						{
							parentCM.SuspendBinding();
							parentCM.ResumeBinding();
						}
						result = GetBindingContextEnsureListManagerSafe(bc, dataSource, dataMember);
					}
				}
			}
			CheckCurrencyManagerListImplementsITypedList(result);
			return result;
		}

		internal static BindingManagerBase GetBindingContextEnsureListManagerSafe(BindingContext bc, object dataSource, string dataMember)
		{
			const int maximumAmountOfRetries = 3;
			for (var tryTimes = 0; ; tryTimes++)
			{
				try
				{
					return bc[dataSource, dataMember];
				}
				catch (NotSupportedException)
				{
					throw new NotSupportedException(String.Format("The list must be an IBindingList to AddNew.\r\nSource: BindingContextExtensions.EnsureListManager\r\nBindingContext:{0}\r\ndataSource Type:{1}{2} ToString:{3}\r\ndataMember:{4}\r\n",
						bc.ToString(),
						dataSource.GetType().ToString(),
						dataSource is BusinessObject ? ("PK: " + ((BusinessObject)dataSource).PK) : string.Empty,
						dataSource.ToString(),
						dataMember));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (!IsConcurrencyExceptionThatShouldBeIgnored(ex))
					{
						throw;
					}
					else if (tryTimes >= maximumAmountOfRetries)
					{
						throw new ArgumentException(string.Format("There is still an exception after many retries with parameters bc[{0}], dataSource[{1}] and dataMember[{2}].",
							bc.GetHashCode(),
							dataSource.GetHashCode(),
							dataMember), ex);
					}

					Thread.Sleep(10);
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		static bool IsConcurrencyExceptionThatShouldBeIgnored(Exception ex)
		{
			return (ex is ArgumentException && ex.Message.Contains("Item has already been added")) ||
				   (ex is InvalidOperationException && ex.Message.StartsWith("Collection was modified; enumeration operation may not execute."));
		}

		#endregion

		static void CheckCurrencyManagerListImplementsITypedList(BindingManagerBase bindingManager)
		{
			CurrencyManager listManager = bindingManager as CurrencyManager;
			if (listManager != null && listManager.List != null && !IsTypedListImplementedAppropriately(listManager.List))
			{
				throw new InvalidOperationException(
						"You must implement ITypedList on collection '" +
						listManager.List.GetType().FullName +
						"' because it's elements of type '" +
						ListUtil.GetListElementType(listManager.List.GetType()) +
						"' implement ICustomTypeDescriptor. This is a known issue in .net data binding.");
			}
		}

		static bool IsTypedListImplementedAppropriately(IList list)
		{
			Type listType = list.GetType();
			bool? result = typedListCheckedTypes[listType];
			if (result == null)
			{
				result =
						list is ITypedList ||
	!typeof(ICustomTypeDescriptor).IsAssignableFrom(ListUtil.GetListElementType(listType));
				typedListCheckedTypes.Add(listType, result);
			}
			return (bool)result;
		}
		[SuppressThreadStaticFieldMessage]
		static readonly LRUCache<Type, bool?> typedListCheckedTypes = new LRUCache<Type, bool?>();

		#endregion
	}
}
