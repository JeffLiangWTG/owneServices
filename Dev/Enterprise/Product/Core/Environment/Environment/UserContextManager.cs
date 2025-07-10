using System;
using CargoWise.Common;

namespace Enterprise.Environment
{
	public delegate void UserContextManagerDispose(IUserContext startingContext, IUserContext endingContext);
	public delegate void OnUserContextChanging(IUserContextChangingEventArgs userContextChangingDetails);

	public sealed class UserContextManager : IDisposable
	{
		readonly IUserContext startingUserContext;
		readonly UserContextManagerDispose userContextDisposeAction;
		readonly OnUserContextChanging onUserContextChanging;
		readonly OnUserContextChanging onUserContextChanged;

		public UserContextManager(OnUserContextChanging onUserContextChanging, OnUserContextChanging onUserContextChanged)
			: this(onUserContextChanging, onUserContextChanged, DefaultUserContextManangerDisposeAction)
		{
		}

		public UserContextManager(
			OnUserContextChanging onUserContextChanging,
			OnUserContextChanging onUserContextChanged,
			UserContextManagerDispose userContextDisposeAction)
		{
			Argument.NotNull(onUserContextChanging, nameof(onUserContextChanging));
			Argument.NotNull(onUserContextChanged, nameof(onUserContextChanged));
			Argument.NotNull(userContextDisposeAction, nameof(userContextDisposeAction));

			this.userContextDisposeAction = userContextDisposeAction;
			this.onUserContextChanging = onUserContextChanging;
			this.onUserContextChanged = onUserContextChanged;
			this.startingUserContext = Env.CurrentUserContext;
			Env.Instance.UserContextChanging += OnUserContextChanging;
			Env.Instance.UserContextChanged += OnUserContextChanged;
		}

		public void Dispose()
		{
			Env.Instance.UserContextChanging -= OnUserContextChanging;
			Env.Instance.UserContextChanged -= OnUserContextChanged;
			userContextDisposeAction?.Invoke(startingUserContext, Env.CurrentUserContext);
		}

		static void DefaultUserContextManangerDisposeAction(IUserContext startingContext, IUserContext endingContext)
		{
		}

		void OnUserContextChanging(object sender, IUserContextChangingEventArgs userContextChangingDetails)
		{
			onUserContextChanging(userContextChangingDetails);
		}

		void OnUserContextChanged(object sender, IUserContextChangingEventArgs userContextChangingEventArgs)
		{
			onUserContextChanged(userContextChangingEventArgs);
		}
	}
}
