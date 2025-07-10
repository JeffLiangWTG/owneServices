using System.Reflection;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Business
{
	public class FieldModule : AutoFieldModule
	{
		public FieldModule() { }

		public override ZString SelectedFieldName
		{
			get { return selectedFieldName; }
		}

		public override ZString SelectedCaption
		{
			get { return selectedCaption; }
		}

		public override ZString SelectedDescription
		{
			get { return selectedDescription; }
		}

		public void SetInfoChain(PropertyInfo[] infoChain)
		{
			if (infoChain == null || infoChain.Length == 0)
			{
				selectedFieldName = ZString.Empty;
				selectedCaption = ZString.Empty;
				selectedDescription = ZString.Empty;
			}
			else
			{
				selectedFieldName = GetFullFieldName(infoChain);

				var data = ResourceStringHelper.GetData(infoChain[infoChain.Length - 1]);

				if (data != null)
				{
					selectedCaption = data.Caption;
					selectedDescription = data.FullDescription;
				}
				else
				{
					selectedCaption = ZString.Empty;
					selectedDescription = ZString.Empty;
				}
			}

			SelectedFieldNameInfo.RefreshBinding();
			SelectedCaptionInfo.RefreshBinding();
			SelectedDescriptionInfo.RefreshBinding();
		}

		public void SetProperty(ICustomProperty property)
		{
			var fieldName = property.Info?.GetCaption();

			selectedFieldName = fieldName;
			selectedCaption = fieldName;
			selectedDescription = fieldName;

			SelectedFieldNameInfo.RefreshBinding();
			SelectedCaptionInfo.RefreshBinding();
			SelectedDescriptionInfo.RefreshBinding();
		}

		#region Implementation

		static string GetFullFieldName(PropertyInfo[] infoChain)
		{
			StringBuilder builder = new StringBuilder();
			string joinner = "";

			foreach (PropertyInfo info in infoChain)
			{
				builder.Append(joinner);
				builder.Append(info.Name);

				switch (ReflectionHelper.Classify(info))
				{
					case PropertyClassification.Updatable:
						joinner = "";
						break;

					case PropertyClassification.FollowSingle:
						joinner = "+";
						break;

					default:
						joinner = ".";
						break;
				}
			}

			if (joinner.Length > 0)
			{
				builder.Append("...");
			}

			return builder.ToString();
		}

		ZString selectedFieldName;
		ZString selectedCaption;
		ZString selectedDescription;

		#endregion
	}
}
