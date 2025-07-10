using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions
{
	public class CompositeHintExtension : HintExtension
	{
		readonly List<IHintExtension> children = new List<IHintExtension>();

		public static CompositeHintExtension Create(params IExtendedControl[] children)
		{
			Argument.NotNull(children, "children");
			Argument.GreaterThanZero(children.Length, "children.Length");

			var result = new CompositeHintExtension();

			foreach (var child in children)
			{
				if (child.Extensions.Supports<IHintExtension>())
				{
					result.Add(child.Extensions.Get<IHintExtension>());
				}
			}

			return result;
		}

		protected virtual void Add(IHintExtension child)
		{
			children.Add(child);
		}

		public override string ShortCaption
		{
			get { return Combine(" & ", child => child.ShortCaption); }
		}

		public override string Caption
		{
			get
			{
				if (!string.IsNullOrEmpty(base.Caption) && base.Caption != MessageForNonDefinedResourceString)
				{
					return base.Caption;
				}

				return Combine(" & ", child => child.Caption);
			}
			set { base.Caption = value; }
		}

		public override string Description
		{
			get
			{
				if (!string.IsNullOrEmpty(base.Description) && base.Description != MessageForNonDefinedResourceString)
				{
					return base.Description;
				}

				return Combine("\r\n", child => child.Description);
			}
			set { base.Description = value; }
		}

		string Combine(string separator, GetPropertyValueDelegate getPropertyValue)
		{
			var result = new StringBuilder();

			foreach (var child in children)
			{
				var value = getPropertyValue(child);

				if (!string.IsNullOrEmpty(value))
				{
					if (result.Length > 0)
					{
						result.Append(separator);
					}

					result.Append(value);
				}
			}

			return result.ToString();
		}

		delegate string GetPropertyValueDelegate(IHintExtension extension);
	}
}
