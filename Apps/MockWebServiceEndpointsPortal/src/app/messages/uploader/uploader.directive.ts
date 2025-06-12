import { Directive, ElementRef } from '@angular/core';

@Directive({
	selector: 'msgUploader',	
})

export class MessagesUploaderDirective {
	constructor(private el: ElementRef) {
	}
}