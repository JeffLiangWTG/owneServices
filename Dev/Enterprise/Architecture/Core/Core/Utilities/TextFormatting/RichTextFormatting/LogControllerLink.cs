using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.Core
{
	[System.Diagnostics.DebuggerDisplay("{Text}({Controller.Name})")]
	public sealed class LogControllerLink : LogHyperlink, IEquatable<LogControllerLink>
	{
		public LogControllerLink(string text, ControllerID controller, ZGuid pk)
			: base(text)
		{
			if (controller == null)
			{
				throw new ArgumentNullException(nameof(controller));
			}

			if (pk.IsEmpty)
			{
				throw new ArgumentException("pk cannot be empty", nameof(pk));
			}

			if (!pk.IsValid)
			{
				throw new ArgumentException("pk must be valid", nameof(pk));
			}

			this.controller = controller;
			this.pk = pk;
		}

		public ControllerID Controller
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return controller; }
		}
		public ZGuid PK
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return pk; }
		}

		public bool Equals(LogControllerLink other)
		{
			return other != null
				&& other.Text == Text
				&& other.controller == controller
				&& other.pk == pk;
		}
		public sealed override bool Equals(object obj)
		{
			return Equals(obj as LogControllerLink);
		}
		public sealed override bool Equals(LogHyperlink other)
		{
			return Equals(other as LogControllerLink);
		}
		public sealed override int GetHashCode()
		{
			return Text.GetHashCode() ^ controller.GetHashCode() ^ pk.GetHashCode();
		}

		readonly ControllerID controller;
		readonly ZGuid pk;
	}
}
