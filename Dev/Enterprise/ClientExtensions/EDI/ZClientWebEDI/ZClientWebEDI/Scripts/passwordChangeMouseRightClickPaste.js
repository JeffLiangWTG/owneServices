var $newPasswordTextbox = $('NewPassword');
$newPasswordTextbox?.addEventListener('paste', (e) => {
	e.preventDefault();

	const text = (e.originalEvent || e).clipboardData.getData('text/plain');
	window.document.execCommand('insertText', false, text);

	return checkPasswordRequirements(text);
});
