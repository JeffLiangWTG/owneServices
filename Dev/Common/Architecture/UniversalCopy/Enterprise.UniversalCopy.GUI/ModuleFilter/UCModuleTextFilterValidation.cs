using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.UniversalCopy.GUI
{
	public class UCModuleTextFilterValidation : ModuleTextFilterValidation
	{
		public UCModuleTextFilterValidation(UCModuleTextFilter parent)
			: base(parent)
		{
		}

		protected override void CheckProperty()
		{
			var parent = (ModuleTextBaseFilter)Parent;

			if (Parent.List is ICodeDescriptionPairList codeDescPairList)
			{
				if (parent.ErrorOnCodeNotPresent)
				{
					ListValidation.ErrorIfInvalidCode(Parent.PropertyInfo, codeDescPairList);
				}
			}
			else
			{
				if (Parent.List is IBusinessObjectCollection collection)
				{
					ListValidation.ErrorIfInvalidCode(Parent.PropertyInfo, collection);
				}
			}

			parent.PropertyValidation?.Invoke(Parent.PropertyInfo);
		}
	}
}
