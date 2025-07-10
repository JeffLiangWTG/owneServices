#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.DialogDefault;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment
{
	public static class UnitTestUserNotificationExtensions
	{
		public static void AddAnswer(this UnitTestUserNotification userTestUserNotification, DialogResult answer)
		{
			userTestUserNotification.AddAnswer((ZDialogResult)answer);
		}

		public static DialogResult ShowOrDefault(this UnitTestUserNotification userTestUserNotification, DialogDefaultContext context, Func<KUserControl> createUserControl)
		{
			Assertion.AssertNotNull(context);
			Assertion.AssertNotNull(createUserControl);

			using (var control = createUserControl())
			{
				ZFormModaliser.ShowDialogAndDispose(new DialogDefaultForm(context, control, (DialogResult)context.DefaultResult));
				userTestUserNotification.SetDefaultable(context, userTestUserNotification.GetResultFor(context.Buttons), control.GetType().Name);
			}

			return (DialogResult)userTestUserNotification.LastMessage.Answer;
		}

		public static DialogResult ShowOrDefault<TDefault>(this UnitTestUserNotification userTestUserNotification, DialogDefaultContext context, ref TDefault dataSource, Func<TDefault, KUserControl> createUserControl, Func<KUserControl, TDefault> getObjectToSerialize = null)
		{
			return ShowOrDefault(userTestUserNotification, context, ref dataSource, createUserControl, null, getObjectToSerialize);
		}

		public static DialogResult ShowOrDefault<TDefault>(this UnitTestUserNotification userTestUserNotification, DialogDefaultContext context, ref TDefault dataSource, Func<TDefault, KUserControl> createUserControl, ISerializer<TDefault> serializer, Func<KUserControl, TDefault> getObjectToSerialize = null)
		{
			Assertion.AssertNotNull("Context", context);
			Assertion.AssertNotNull("CreateUserControl", createUserControl);

			//Test all the passed in items
			//Test serialisation

			serializer = serializer ?? new ZXmlSerializerWrapper<TDefault>();
			var element = serializer.Serialize(dataSource);
			var result = serializer.Deserialize(element);

			Assertion.AssertEquals("Should get out what we put in", dataSource, result);

			//Test delegates
			using (var control = createUserControl(result))
			{
				Assertion.AssertNotNull("CreateUserControl Result mustn't be null", control);

				if (getObjectToSerialize != null)
				{
					getObjectToSerialize(control);
				}

				//IDialogDefaultControlMembers
				var dialogDefaultMembers = control as IDialogDefaultControlMembers;
				if (dialogDefaultMembers != null)
				{
					dialogDefaultMembers.SetReadOnly(false, userTestUserNotification.GetResultFor(context.Buttons));
					dialogDefaultMembers.SetReadOnly(true, userTestUserNotification.GetResultFor(context.Buttons));
				}
				else if (context.Buttons == null)
				{
					Assertion.Fail("Please implement IDialogDefaultControlMembers so that if your dialog is read only the user can still close it");
				}
			}

			if (userTestUserNotification.NextDataSourceResponse.Count == 0)
			{
				throw new InvalidOperationException("Cannot test for a response without setting one");
			}

			dataSource = (TDefault)userTestUserNotification.NextDataSourceResponse.Dequeue();
			using (var control = createUserControl(dataSource))
			{
				userTestUserNotification.SetDefaultable(context, userTestUserNotification.GetResultFor(context.Buttons), control.GetType().Name);
			}

			return (DialogResult)userTestUserNotification.LastMessage.Answer;
		}
	}
}
#endif
