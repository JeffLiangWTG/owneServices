using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CargoWise.Common
{
	/// <summary>
	/// A delegate whose target is weak referenced.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public class WeakTargetDelegate<T> where T : class
	{
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d")]
		public WeakTargetDelegate(T d)
		{
			if ((d as Delegate) == null)
			{
				throw new ArgumentException("Invalid argument.", nameof(d));
			}

			if ((d as Delegate).GetInvocationList() == null)
			{
				throw new ArgumentException("Invalid argument.", nameof(d));
			}

			if (!typeof(Delegate).IsAssignableFrom(typeof(T)))
			{
				throw new ArgumentException("T must be a delegate type", nameof(d));
			}
			Initialize(d as Delegate);
		}

		void Initialize(Delegate d)
		{
			Argument.NotNull(d, nameof(d)); // Suggested By ReviewBot 
			if (d.GetInvocationList().Length > 1)
			{
				throw new ArgumentException("A multicast delegate is not supported", nameof(d));
			}
			targetRef = d.Target == null ? null : new WeakReference(d.Target);
			method = d.Method;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d")]
		public static implicit operator T(WeakTargetDelegate<T> d)
		{
			Argument.NotNull(d, nameof(d));
			return d.ToDelegate();
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "d")]
		public static implicit operator WeakTargetDelegate<T>(T d)
		{
			return new WeakTargetDelegate<T>(d);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator ==(WeakTargetDelegate<T> lhs, WeakTargetDelegate<T> rhs)
		{
			return object.Equals(lhs, rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator !=(WeakTargetDelegate<T> lhs, WeakTargetDelegate<T> rhs)
		{
			return !(lhs == rhs);
		}

		public override bool Equals(object obj)
		{
			WeakTargetDelegate<T> rhs = obj as WeakTargetDelegate<T>;
			if (rhs == null)
			{
				return false;
			}
			return
				method == rhs.method && ((targetRef == null && rhs.targetRef == null) || (targetRef != null && rhs.targetRef != null && targetRef.Target == rhs.targetRef.Target));
		}

		public override int GetHashCode()
		{
			return method.GetHashCode();
		}

		public T ToDelegate()
		{
			T result = null;
			object target = targetRef == null ? null : targetRef.Target;
			if (targetRef == null || target != null)
			{
				result = System.Delegate.CreateDelegate(typeof(T), target, method) as object as T;
			}
			return result;
		}

		public bool IsAlive
		{
			get { return targetRef == null || targetRef.IsAlive; }
		}

		WeakReference targetRef;
		MethodInfo method;
	}
}
