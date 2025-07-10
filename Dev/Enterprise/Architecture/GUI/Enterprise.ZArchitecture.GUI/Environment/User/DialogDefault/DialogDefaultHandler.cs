using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Core.DialogDefault
{
	/// <summary>
	///     Creates the arbitrary KUserControl to be displayed on the messagebox. It will be bound to dataSource in the
	///     calling function.
	///     Delegate for lazy init.
	/// </summary>
	/// <param name="dataSource">The data source for this object. The creator function should bind to the given object</param>
	public delegate KUserControl DefaultMessageBoxContentCreator<in T>(T dataSource);

	public class DialogDefaultHandler
	{
		public DialogDefaultHandler(BusinessObjectFactory factory)
		{
			_factory = Argument.NotNull(factory, nameof(factory));
		}

		/// <summary>
		///     Shows the given message box, with a label of the given message
		/// </summary>
		/// <param name="context">The context</param>
		/// <param name="message">The message/question for the user</param>
		/// <returns>
		///     The DialogResult that reflects the clicked button, or DialogResult.Cancel
		/// </returns>
		public DialogResult ShowOrDefault(DialogDefaultContext context, MultilingualString message)
		{
			return ShowOrDefault(context, message.ToString());
		}

		public DialogResult ShowOrDefault(DialogDefaultContext context, string message)
		{
			//Creates a simple control with only text. Ignores dataSource because it does not matter
			Func<KUserControl> createDialog = () =>
			{
				var container = new ZUserControl { AutoSize = true, BackColor = Color.White };

				var label = new ZLabel { Text = message, AutoSize = true };
				container.Controls.Add(label);
				container.Size = ControlDpiScalingHelper.NewScaledSize(label.Size, false);

				return container;
			};

			return ShowOrDefault(context, createDialog);
		}

		public DialogResult ShowOrDefault(DialogDefaultContext context, Func<KUserControl> createUserControl)
		{
			object o = null;
			return ShowOrDefault(context, ref o, (_) => createUserControl(), serializer: null);
		}

		public DialogResult ShowOrDefault<T>(DialogDefaultContext context, ref T dataSource, DefaultMessageBoxContentCreator<T> createUserControl, Func<KUserControl, T> getObjectToSerialize = null)
		{
			return ShowOrDefault(context, ref dataSource, createUserControl, new ZXmlSerializerWrapper<T>(), getObjectToSerialize);
		}

		/// <summary>
		/// Tries to retrieve defaults for this user, if there is none it will show the dialog
		/// 
		/// If there is no defaults, or deserialize throws an exception than the dataSource will remain as what is passed, so generic defaults can be added there
		/// </summary>
		/// <param name="context">The message box context</param>
		/// <param name="dataSource">The data source for the KForm. NOT NULL</param>
		/// <param name="createUserControl">
		///     A function that constructs the user control to be added to the form.
		///     This will not be executed if a suitable default is present
		/// </param>
		/// <param name="serializer">Serialize/Deserialize the object</param>
		/// <param name="getDataToSerialize">
		///		Called after the form is closed but before it is disposed. It takes the form and returns the data which should be serialized (for example, the DataSource).
		///		If it is left null than it will serialize and save the param 'dataSource'.
		/// </param>
		/// <returns>
		///     The DialogResult that reflects the clicked button, or DialogResult.Cancel
		/// </returns>
		public DialogResult ShowOrDefault<T>(DialogDefaultContext context, ref T dataSource,
			DefaultMessageBoxContentCreator<T> createUserControl, ISerializer<T> serializer, Func<KUserControl, T> getDataToSerialize = null)
		{
			var defaults = GetDefaultsFor(context);
			var defaultResult = DialogResult.None;

			if (defaults != null)
			{
				//If we fail to deserialize these defaults than we stop trying to use them
				if (!TryDeserialize(context, ref dataSource, out defaultResult, serializer, defaults))
				{
					defaults = null;
				}
			}

			return ShouldShowMessageBox(context, defaults) ? ShowMessageBox(context, ref dataSource, createUserControl(dataSource), defaultResult, defaults, serializer, getDataToSerialize) : defaultResult;
		}

		bool TryDeserialize<T>(DialogDefaultContext context, ref T dataSource, out DialogResult defaultResult, ISerializer<T> serializer, StmDialogDefault defaults)
		{
			try
			{
				var deserialized = Deserialize(serializer, defaults.SDD_SerializedDefaults);

				defaultResult = deserialized.Item1;
				dataSource = deserialized.Item2;

				if (!context.IsValidResult((ZDialogResult)defaultResult))
				{
					throw new InvalidOperationException(
						String.Format("Had result {0} saved for {1}, which is not valid for {2}.",
							Enum.GetName(typeof(DialogResult), defaultResult),
							context.DialogIdentifier,
							(context.Buttons == null) ? "null" : Enum.GetName(typeof(MessageBoxButtons), context.Buttons)));
				}

				return true;
			}
			catch (Exception ex) //Includes problems in deserialize func
			{
				if (ex.IsCriticalException()) { throw; }

				ErrorReporter.ReportOnce("An exception was thrown when retrieving a default for " + context.DialogIdentifier, ex);

				defaultResult = DialogResult.None;
				return false;
			}
		}

		bool ShouldShowMessageBox(DialogDefaultContext context, StmDialogDefault defaults)
		{
			return defaults == null || defaults.SDD_ShowDialog_Base || (defaults.SDD_Level != DialogDefaultLevel.Codes.User && !context.GlobalOnly) || Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
		}

		bool CurrentUserCanModify(StmDialogDefault defaults)
		{
			return defaults == null ||
					 CanCreateAndModifyGlobalDialogDefaultsCheckpoint.IsAllowed ||
					 (defaults.ParentOverride == null && defaults.SDD_Level == DialogDefaultLevel.Codes.User);
		}

		DialogResult ShowMessageBox<T>(DialogDefaultContext context, ref T dataSource, KUserControl control, DialogResult defaultResult, StmDialogDefault currentDefaults, ISerializer<T> serializer, Func<KUserControl, T> getDataToSerialize)
		{
			using (var dialog = new DialogDefaultForm(context, control, defaultResult, !CurrentUserCanModify(currentDefaults)))
			{
				var result = ZFormModaliser.ShowMessageBoxWithoutDispose(dialog);

				if (!context.DialogResultsToNotSave.Contains((ZDialogResult)result))
				{
					if (getDataToSerialize != null)
					{
						dataSource = getDataToSerialize(control);
					}

					var saveOptions = dialog.Options;
#if DEBUG
					if (ForcedSaveOptions != null)
					{
						saveOptions = ForcedSaveOptions;
					}
#endif

					#region WI00169022 & 00971928

					// This piece of code is added to get more information for issues 00971928 & 01078965, if there's no more exception reported, it can be removed safely.
					if (saveOptions == null)
					{
						   var message = FormattableString.Invariant($@"dialog.Options is null:
		dialog.IsDesignMode() = [{dialog.IsDesignMode()}]
		dialog.BusinessEntity = [{dialog.BusinessEntity?.GetType().FullName}]
		dialog.DataSource = [{dialog.DataSource?.GetType().FullName}]
		dialog.IsDisposed = [{dialog.IsDisposed}]"); // Message are only sent back to us
						ErrorReporter.ReportOnce("dialog.OptionsIsNullException", message);
					}

					#endregion

					if (saveOptions != null && saveOptions.SaveNewDefaults)
					{
						var serialisedResults = Serialize(serializer, dataSource, result);
						UpdateDefaults(context, saveOptions, currentDefaults, serialisedResults);

						if (!saveOptions.IsUserLevel && saveOptions.SaveForMeAsWell)
						{
							var clone = saveOptions.Clone();
							clone.Level = DialogDefaultLevel.Codes.User;
							UpdateDefaults(context, clone, currentDefaults, serialisedResults);
						}
					}
				}

				return result;
			}
		}

		#region Storing/retrieving

		readonly BusinessObjectFactory _factory;

		BusinessObjectFactory Factory
		{
			get { return _factory; }
		}

		/// <summary>
		///     Saves the given defaults to the DB, either overriding whats there or creating a new one where relevant
		/// </summary>
		void UpdateDefaults(DialogDefaultContext context, DialogDefaultSaveOptions options, StmDialogDefault currentDefaults, string serializedDefaults)
		{
			var newDefaultsAppliesToSimilar = options.SaveForAllContexts && context.NullContextAllowed;

			var isSavingNullContextWhenNonNullExists = newDefaultsAppliesToSimilar && currentDefaults != null && !currentDefaults.AppliesToSimilarDialogs;
			if (isSavingNullContextWhenNonNullExists)
			{
				currentDefaults.Delete();
				currentDefaults = null;
			}

			//If the previous defaults did not exist or were not owned by us
			if (currentDefaults == null || !currentDefaults.SDD_Level.Equals(options.Level) || currentDefaults.AppliesToSimilarDialogs != newDefaultsAppliesToSimilar)
			{
				currentDefaults = TryGetDefaultWithSameOwner(context, options);
			}

			if (currentDefaults == null)
			{
				currentDefaults = Factory.New<StmDialogDefault>();
				currentDefaults.SDD_DialogIdentifier = context.DialogIdentifier;
				currentDefaults.SDD_Context = newDefaultsAppliesToSimilar ? ZBlob.Empty : context.Context;
				currentDefaults.SDD_Level = options.Level;
				currentDefaults.SDD_Owner =
					(options.Level == DialogDefaultLevel.Codes.User) ? GetCurrentUser() :
					(options.Level == DialogDefaultLevel.Codes.Company) ? GetCurrentCompany() : ZGuid.Empty;
			}

			currentDefaults.SDD_Caption = context.Caption;
			currentDefaults.SDD_ShowDialog = !options.IsUserLevel || options.KeepShowingDialog;
			currentDefaults.SDD_OverrideAllChildLevels = options.OverridePersonal;
			currentDefaults.SDD_SerializedDefaults = serializedDefaults;

			Factory.Save();
		}

		StmDialogDefault TryGetDefaultWithSameOwner(DialogDefaultContext context, DialogDefaultSaveOptions options)
		{
			var query = new ZQuery(StmDialogDefaultSchema.SDD_DialogIdentifier, context.DialogIdentifier);

			if (options.Level != DialogDefaultLevel.Codes.Global)
			{
				query.AddToFilter(StmDialogDefaultSchema.SDD_Owner, (options.Level == DialogDefaultLevel.Codes.User) ? GetCurrentUser() : GetCurrentCompany());
			}

			var contextQuery = options.SaveForAllContexts || context.Context.IsEmpty ?
				new ZQuery(StmDialogDefaultSchema.SDD_Context, null) :
				new ZDBOnlyQuery(typeof(StmDialogDefault)).AddToFilter(StmDialogDefaultSchema.SDD_Context, context.Context);

			return Factory.LoadTop1<StmDialogDefault>(query.AddToFilter(contextQuery));
		}

		/// <summary>
		///     Gets the defaults for the current user for the given dialog
		/// </summary>
		StmDialogDefault GetDefaultsFor(DialogDefaultContext context)
		{
			ZGuid user = GetCurrentUser(), company = GetCurrentCompany();

			return GetDefaultsFor(context, user, company);
		}

		StmDialogDefault GetDefaultsFor(DialogDefaultContext context, ZGuid userPk, ZGuid companyPk)
		{
			var defaults = GetAllApplicableDefaultsSortedBySpecificity(context, userPk, companyPk);

			var forcedOverride = defaults.LastOrDefault(def => def.SDD_OverrideAllChildLevels);
			var prefferedDefault = defaults.FirstOrDefault();

			if (forcedOverride == null || prefferedDefault == forcedOverride || CanCreateAndModifyGlobalDialogDefaultsCheckpoint.IsAllowed)
			{
				return prefferedDefault;
			}
			prefferedDefault.ParentOverride = forcedOverride;

			return (forcedOverride.SDD_SerializedDefaults == prefferedDefault.SDD_SerializedDefaults)
				? prefferedDefault
				: forcedOverride;
		}

		IEnumerable<StmDialogDefault> GetAllApplicableDefaultsSortedBySpecificity(DialogDefaultContext context, ZGuid userPk, ZGuid companyPk)
		{
			var companyFilter = new ZQuery()
				.AddToFilter(StmDialogDefaultSchema.SDD_Owner, userPk)
				.AddToFilter(StmDialogDefaultSchema.SDD_Level, DialogDefaultLevel.Codes.User);

			var userFilter = new ZQuery()
				.AddToFilter(StmDialogDefaultSchema.SDD_Owner, companyPk)
				.AddToFilter(StmDialogDefaultSchema.SDD_Level, DialogDefaultLevel.Codes.Company);

			var globalFilter = new ZQuery(StmDialogDefaultSchema.SDD_Level, DialogDefaultLevel.Codes.Global);

			var contextFilter = new ZQuery(StmDialogDefaultSchema.SDD_Context, null);
			if (!context.Context.IsEmpty)
			{
				var specificContext = new ZDBOnlyQuery(typeof(StmDialogDefault));
				specificContext.AddToFilter(StmDialogDefaultSchema.SDD_Context, context.Context);

				contextFilter.AddToFilter(specificContext, JoinCondition.Or);
			}

			// (DialogId == context.ID) && (context in (null, context)) && (owner is me | my Company | Global)
			var query = new ZQuery(StmDialogDefaultSchema.SDD_DialogIdentifier, context.DialogIdentifier)
				.AddToFilter(contextFilter)
				.AddToFilter(globalFilter.AddToFilter(companyFilter, JoinCondition.Or).AddToFilter(userFilter, JoinCondition.Or));

			return Factory.Load<StmDialogDefault>(query).OrderBy(def => def);
		}

		#endregion

		#region Serialization

		#region serialize object and default

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Xml element name.")]
		const string RootName = "root";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Xml element name.")]
		const string ResultName = "Result";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Xml element name.")]
		const string DefaultsName = "Defaults";

		string Serialize<T>(ISerializer<T> serializer, T defaults, DialogResult result)
		{
			var root = new XElement(RootName, new XElement(ResultName, result));

			if (serializer != null)
			{
				root.Add(new XElement(DefaultsName, serializer.Serialize(defaults)));
			}

			return root.ToString(SaveOptions.DisableFormatting);
		}

		Tuple<DialogResult, T> Deserialize<T>(ISerializer<T> serializer, string serializedDefaults)
		{
			var root = XElement.Parse(serializedDefaults);

			var result = (DialogResult)Enum.Parse(typeof(DialogResult), root.Element(ResultName).Value);

			var defaults = default(T);
			if (serializer != null)
			{
				var defaultsNode = root.Element(DefaultsName).Elements().First();
				defaults = serializer.Deserialize(defaultsNode);
			}

			return Tuple.Create(result, defaults);
		}

		#endregion

		#endregion

#if DEBUG

		#region exposed for testing

		internal DialogDefaultSaveOptions ForcedSaveOptions { get; set; }

		internal StmDialogDefault GetDefaultsForExposed(DialogDefaultContext context)
		{
			return GetDefaultsFor(context);
		}

		internal StmDialogDefault GetDefaultsForExposed(DialogDefaultContext context, ZGuid userPk, ZGuid companyPk)
		{
			return GetDefaultsFor(context, userPk, companyPk);
		}

		internal IEnumerable<StmDialogDefault> GetAllApplicableDefaultsSortedBySpecificityExposed(DialogDefaultContext context)
		{
			return GetAllApplicableDefaultsSortedBySpecificity(context, GetCurrentUser(), GetCurrentCompany());
		}

		public string SerializeExposed<T>(DialogResult result, T defaults, ISerializer<T> serializer = null)
		{
			return Serialize(serializer, defaults, result);
		}

		public Tuple<DialogResult, T> DeserializeExposed<T>(string xml, ISerializer<T> serializer = null)
		{
			return Deserialize(serializer, xml);
		}

		public ZGuid? CurrentUserGuidOverride { get; set; }
		public ZGuid? CurrentCompanyGuidOverride { get; set; }

		#endregion

#endif

		#region current company and user

		ZGuid GetCurrentCompany()
		{
			return
#if DEBUG
			CurrentCompanyGuidOverride ??
#endif
			EnvProxy.Instance.CurrentCompany?.PK ?? ZGuid.Empty;
		}

		ZGuid GetCurrentUser()
		{
			return
#if DEBUG
			CurrentUserGuidOverride ??
#endif
			EnvProxy.Instance.CurrentUser?.PK ?? ZGuid.Empty;
		}

#if DEBUG
		internal
#endif
		static ISecurityCheckpoint CanCreateAndModifyGlobalDialogDefaultsCheckpoint
		{
			get { return EnvProxy.Instance.Security.FindCheckPoint("CanCreateAndModifyGlobalDialogDefaults"); }
		}
		#endregion

	}
}
