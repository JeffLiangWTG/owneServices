using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Registry.GUI
{
	public class CodeFindBoxBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string SelectedCode = "SelectedCode";
		}

		#endregion

		public CodeFindBoxBusinessObject(IRegistryEditorInfo editorInfo)
		{
			CodeFindBoxRegistryEditorInfo codeEditorInfo = (CodeFindBoxRegistryEditorInfo)editorInfo;
			fChoicesCollection = codeEditorInfo.GetFindBoxCollection(new BusinessObjectFactory());
			fModuleID = codeEditorInfo.ModuleID;
		}

		#region SelectedCode

		[BusinessObjectTestExclude]
		public ZString SelectedCode
		{
			get { return fSelectedCode; }
			set
			{
				if (SelectedCode != value)
				{
					fSelectedCode = value;
					HasChanges = true;
				}

				if (!IsValidationSuspended)
				{
					ValidateSelectedCode();
				}

				SelectedCodeInfo.RefreshBinding();
			}
		}
		ZString fSelectedCode;

		public virtual void ValidateSelectedCode()
		{
			SelectedCodeInfo.ClearAllNotifications();
			ListValidation.MessageErrorIfInvalidCode(SelectedCodeInfo, ChoicesCollection);
		}

		public ZPropertyInfo SelectedCodeInfo
		{
			get { return GetZPropertyInfo(Schema.SelectedCode); }
		}

		#endregion

		public IBusinessObjectCollection ChoicesCollection
		{
			get { return fChoicesCollection; }
		}

		public ModuleIdentifier ModuleID
		{
			get { return fModuleID; }
		}

		readonly IBusinessObjectCollection fChoicesCollection;
		readonly ModuleIdentifier fModuleID;
	}
}
