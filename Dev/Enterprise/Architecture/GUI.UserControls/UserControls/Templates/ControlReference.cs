using System;

namespace Enterprise.ZArchitecture.GUI
{
	public class ControlReference : IPanelLayoutPart
	{
		public ControlReference(IControlBag bag, string controlName)
		{
			Bag = bag;
			ControlName = controlName;
		}

		public IControlBag Bag { get; }
		public string ControlName { get; }

		public string Name => ControlName;

		public override string ToString()
		{
			return FormattableString.Invariant($"{Bag?.GetType().Name}.{ControlName}"); // For debugging
		}

		protected bool Equals(ControlReference other)
		{
			return Equals(Bag, other.Bag) && string.Equals(ControlName, other.ControlName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((ControlReference)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Bag != null ? Bag.GetHashCode() : 0) * 397) ^ (ControlName != null ? ControlName.GetHashCode() : 0);
			}
		}
	}
}
